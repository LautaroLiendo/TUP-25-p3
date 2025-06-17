using Microsoft.EntityFrameworkCore;
using TiendaRopaApi.Data;
using TiendaRopaApi.Models;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RopaContext>(opt =>
    opt.UseSqlite("Data Source=ropa.db"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5177", "https://localhost:7221")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("PermitirFrontend");

using var scope = app.Services.CreateScope();
var contexto = scope.ServiceProvider.GetRequiredService<RopaContext>();
contexto.Database.EnsureCreated();

if (!contexto.Prendas.Any())
{
    contexto.Prendas.AddRange(new List<Prenda>
    {
        new() { Titulo = "Camisa Casual", Descripcion = "Camisa de algodón azul", Precio = 25.50m, Inventario = 30, Imagen = "https://example.com/camisa-casual.jpg" },
        new() { Titulo = "Pantalón Jeans", Descripcion = "Jeans clásico oscuro", Precio = 40.00m, Inventario = 25, Imagen = "https://example.com/pantalon-jeans.jpg" },
        new() { Titulo = "Chaqueta Cuero", Descripcion = "Chaqueta de cuero sintético negra", Precio = 75.00m, Inventario = 10, Imagen = "https://example.com/chaqueta-cuero.jpg" },
        new() { Titulo = "Vestido Verano", Descripcion = "Vestido ligero y fresco", Precio = 45.00m, Inventario = 20, Imagen = "https://example.com/vestido-verano.jpg" },
        new() { Titulo = "Bufanda Lana", Descripcion = "Bufanda de lana cálida", Precio = 15.00m, Inventario = 40, Imagen = "https://example.com/bufanda-lana.jpg" },
        new() { Titulo = "Zapatos Deportivos", Descripcion = "Zapatos para correr cómodos", Precio = 60.00m, Inventario = 35, Imagen = "https://example.com/zapatos-deportivos.jpg" },
        new() { Titulo = "Gorra Urbana", Descripcion = "Gorra con logo bordado", Precio = 12.00m, Inventario = 50, Imagen = "https://example.com/gorra-urbana.jpg" },
        new() { Titulo = "Cinturón Cuero", Descripcion = "Cinturón marrón para pantalones", Precio = 18.00m, Inventario = 45, Imagen = "https://example.com/cinturon-cuero.jpg" },
        new() { Titulo = "Camisa Formal", Descripcion = "Camisa blanca para oficina", Precio = 30.00m, Inventario = 20, Imagen = "https://example.com/camisa-formal.jpg" },
        new() { Titulo = "Polo Manga Corta", Descripcion = "Polo cómodo y casual", Precio = 22.00m, Inventario = 30, Imagen = "https://example.com/polo-manga-corta.jpg" }
    });
    contexto.SaveChanges();
}

// Carrito en memoria (id -> lista de prendas con cantidades)
var carritosActivos = new ConcurrentDictionary<string, List<DetalleOrden>>();

app.MapGet("/", () => Results.Ok(new { Mensaje = "API de Tienda de Ropa", Fecha = DateTime.Now }));

app.MapGet("/prendas", async (RopaContext ctx) => await ctx.Prendas.ToListAsync());

app.MapGet("/prendas/buscar/{termino}", async (RopaContext ctx, string termino) =>
{
    var texto = termino.ToLower();
    return await ctx.Prendas
        .Where(p => p.Titulo.ToLower().Contains(texto) || p.Descripcion.ToLower().Contains(texto))
        .ToListAsync();
});

app.MapPost("/carrito/nuevo", () =>
{
    var idCarrito = Guid.NewGuid().ToString();
    carritosActivos[idCarrito] = new List<DetalleOrden>();
    return Results.Ok(new { idCarrito });
});

app.MapGet("/carrito/{idCarrito}", async (string idCarrito, RopaContext ctx) =>
{
    if (!carritosActivos.TryGetValue(idCarrito, out var items))
        return Results.NotFound(new { error = "Carrito no encontrado" });

    var detalles = new List<object>();
    foreach (var item in items)
    {
        var prenda = await ctx.Prendas.FindAsync(item.PrendaId);
        detalles.Add(new
        {
            PrendaId = item.PrendaId,
            NombrePrenda = prenda?.Titulo ?? "Prenda desconocida",
            Cantidad = item.Cantidad,
            PrecioUnitario = item.PrecioUnitario
        });
    }

    return Results.Ok(detalles);
});

app.MapPut("/carrito/{idCarrito}/agregar/{prendaId}", async (string idCarrito, int prendaId, int cantidad, RopaContext ctx) =>
{
    if (cantidad <= 0) return Results.BadRequest(new { error = "Cantidad inválida" });

    var prenda = await ctx.Prendas.FindAsync(prendaId);
    if (prenda == null) return Results.NotFound(new { error = "Prenda no encontrada" });

    if (prenda.Inventario < cantidad)
        return Results.BadRequest(new { error = "Inventario insuficiente" });

    var carrito = carritosActivos.GetOrAdd(idCarrito, _ => new List<DetalleOrden>());
    var item = carrito.FirstOrDefault(i => i.PrendaId == prendaId);
    if (item == null)
        carrito.Add(new DetalleOrden { PrendaId = prendaId, Cantidad = cantidad, PrecioUnitario = prenda.Precio });
    else
        item.Cantidad = cantidad;

    return Results.Ok(carrito);
});

app.MapDelete("/carrito/{idCarrito}/quitar/{prendaId}", (string idCarrito, int prendaId, int cantidad = 0) =>
{
    if (!carritosActivos.TryGetValue(idCarrito, out var carrito))
        return Results.NotFound(new { error = "Carrito no encontrado" });

    var item = carrito.FirstOrDefault(i => i.PrendaId == prendaId);
    if (item == null)
        return Results.NotFound(new { error = "Prenda no encontrada en el carrito" });

    if (cantidad <= 0 || cantidad >= item.Cantidad)
        carrito.Remove(item);
    else
        item.Cantidad -= cantidad;

    return Results.Ok(carrito);
});

app.MapPut("/carrito/{idCarrito}/confirmar", async (string idCarrito, RopaContext ctx, HttpContext http) =>
{
    try
    {
        using var reader = new StreamReader(http.Request.Body);
        var json = await reader.ReadToEndAsync();

        var cliente = System.Text.Json.JsonSerializer.Deserialize<ClienteInfo>(json);
        if (cliente == null || string.IsNullOrWhiteSpace(cliente.Nombre) || string.IsNullOrWhiteSpace(cliente.Apellido) || string.IsNullOrWhiteSpace(cliente.Correo))
            return Results.BadRequest(new { error = "Información de cliente incompleta" });

        if (!carritosActivos.TryGetValue(idCarrito, out var carrito))
            return Results.BadRequest(new { error = "Carrito no encontrado" });

        if (carrito.Count == 0)
            return Results.BadRequest(new { error = "Carrito vacío" });

        foreach (var item in carrito)
        {
            var prenda = await ctx.Prendas.FindAsync(item.PrendaId);
            if (prenda == null) return Results.BadRequest(new { error = $"Prenda {item.PrendaId} no existe" });
            if (prenda.Inventario < item.Cantidad)
                return Results.BadRequest(new { error = $"Inventario insuficiente para {prenda.Titulo}" });
        }

        decimal total = 0;
        var orden = new Orden
        {
            FechaOrden = DateTime.Now,
            NombreCliente = cliente.Nombre,
            ApellidoCliente = cliente.Apellido,
            EmailCliente = cliente.Correo,
            Detalles = new List<DetalleOrden>()
        };

        foreach (var item in carrito)
        {
            var prenda = await ctx.Prendas.FindAsync(item.PrendaId);
            prenda.Inventario -= item.Cantidad;
            total += prenda.Precio * item.Cantidad;

            orden.Detalles.Add(new DetalleOrden
            {
                PrendaId = prenda.PrendaId,
                Cantidad = item.Cantidad,
                PrecioUnitario = prenda.Precio
            });
        }
        orden.Total = total;

        ctx.Ordenes.Add(orden);
        await ctx.SaveChangesAsync();

        carritosActivos[idCarrito] = new List<DetalleOrden>();
        return Results.Ok(new { orden.OrdenId, orden.Total });
    }
    catch
    {
        return Results.BadRequest(new { error = "Error al procesar la orden" });
    }
});

app.Run();
