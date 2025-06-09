using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// CORS para permitir que Blazor (cliente) acceda a esta API
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Configurar EF Core con SQLite
builder.Services.AddDbContext<TiendaDb>(options =>
    options.UseSqlite("Data Source=tienda.db"));

var app = builder.Build();
app.UseCors();

// ─────────────── ENDPOINTS ───────────────

// Obtener productos (con búsqueda opcional)
app.MapGet("/productos", async (string? buscar, TiendaDb db) =>
{
    var query = db.Productos.AsQueryable();

    if (!string.IsNullOrWhiteSpace(buscar))
        query = query.Where(p => p.Nombre.Contains(buscar));

    return await query.ToListAsync();
});

// ─────────────── Carritos en memoria ───────────────
var carritos = new Dictionary<Guid, List<ItemCompra>>();

// Crear un carrito
app.MapPost("/carritos", () =>
{
    var id = Guid.NewGuid();
    carritos[id] = new List<ItemCompra>();
    return Results.Ok(id);
});

// Obtener contenido del carrito
app.MapGet("/carritos/{id}", (Guid id) =>
{
    if (!carritos.ContainsKey(id))
        return Results.NotFound();

    return Results.Ok(carritos[id]);
});

// Vaciar carrito
app.MapDelete("/carritos/{id}", (Guid id) =>
{
    if (!carritos.ContainsKey(id))
        return Results.NotFound();

    carritos[id].Clear();
    return Results.NoContent();
});

// Agregar producto al carrito
app.MapPut("/carritos/{id}/{productoId}", async (Guid id, int productoId, TiendaDb db) =>
{
    if (!carritos.ContainsKey(id))
        return Results.NotFound("Carrito no encontrado");

    var producto = await db.Productos.FindAsync(productoId);
    if (producto is null)
        return Results.NotFound("Producto no encontrado");

    if (producto.Stock <= 0)
        return Results.BadRequest("Sin stock");

    var carrito = carritos[id];
    var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);

    if (item is null)
    {
        carrito.Add(new ItemCompra
        {
            ProductoId = productoId,
            Producto = producto,
            Cantidad = 1,
            PrecioUnitario = producto.Precio
        });
    }
    else
    {
        if (item.Cantidad >= producto.Stock)
            return Results.BadRequest("No hay más stock");

        item.Cantidad++;
    }

    return Results.Ok(carrito);
});

// Quitar producto del carrito
app.MapDelete("/carritos/{id}/{productoId}", (Guid id, int productoId) =>
{
    if (!carritos.ContainsKey(id))
        return Results.NotFound();

    var carrito = carritos[id];
    var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);

    if (item is null)
        return Results.NotFound();

    item.Cantidad--;

    if (item.Cantidad <= 0)
        carrito.Remove(item);

    return Results.Ok(carrito);
});

// Confirmar compra
app.MapPut("/carritos/{id}/confirmar", async (Guid id, ClienteDto cliente, TiendaDb db) =>
{
    if (!carritos.ContainsKey(id))
        return Results.NotFound();

    var carrito = carritos[id];
    if (!carrito.Any())
        return Results.BadRequest("Carrito vacío");

    foreach (var item in carrito)
    {
        var producto = await db.Productos.FindAsync(item.ProductoId);
        if (producto == null || producto.Stock < item.Cantidad)
            return Results.BadRequest($"Stock insuficiente para {producto?.Nombre}");
    }

    var total = carrito.Sum(i => i.Cantidad * i.PrecioUnitario);

    var compra = new Compra
    {
        Fecha = DateTime.Now,
        Total = total,
        NombreCliente = cliente.Nombre,
        ApellidoCliente = cliente.Apellido,
        EmailCliente = cliente.Email,
        Items = new List<ItemCompra>()
    };

    foreach (var item in carrito)
    {
        var producto = await db.Productos.FindAsync(item.ProductoId);
        if (producto != null)
        {
            producto.Stock -= item.Cantidad;

            compra.Items.Add(new ItemCompra
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario
            });
        }
    }

    db.Compras.Add(compra);
    await db.SaveChangesAsync();

    carrito.Clear();

    return Results.Ok(compra);
});

// ─────────────── Inicializar base de datos ───────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TiendaDb>();
    db.Database.EnsureCreated();

    if (!db.Productos.Any())
    {
        db.Productos.AddRange(
            new Producto { Nombre = "Camisa Casual", Descripcion = "Algodón, manga larga", Precio = 2500, Stock = 20, ImagenUrl = "/img/camisa_casual.jpg" },
            new Producto { Nombre = "Pantalón Jeans", Descripcion = "Denim azul oscuro", Precio = 3500, Stock = 15, ImagenUrl = "/img/jeans.jpg" },
            new Producto { Nombre = "Vestido Veraniego", Descripcion = "Fresco y ligero", Precio = 4200, Stock = 10, ImagenUrl = "/img/vestido_verano.jpg" },
            new Producto { Nombre = "Chaqueta de Cuero", Descripcion = "Estilo biker sintético", Precio = 7800, Stock = 5, ImagenUrl = "/img/chaqueta_cuero.jpg" },
            new Producto { Nombre = "Sudadera con Capucha", Descripcion = "Felpa suave unisex", Precio = 3200, Stock = 12, ImagenUrl = "/img/sudadera.jpg" },
            new Producto { Nombre = "Falda Plisada", Descripcion = "Midi elegante", Precio = 2900, Stock = 8, ImagenUrl = "/img/falda_plisada.jpg" },
            new Producto { Nombre = "Camisa de Vestir", Descripcion = "Formal blanca", Precio = 2700, Stock = 25, ImagenUrl = "/img/camisa_vestir.jpg" },
            new Producto { Nombre = "Chaleco Deportivo", Descripcion = "Acolchado para el frío", Precio = 5000, Stock = 7, ImagenUrl = "/img/chaleco.jpg" },
            new Producto { Nombre = "Leggings Deportivos", Descripcion = "Compresión y confort", Precio = 2300, Stock = 18, ImagenUrl = "/img/leggings.jpg" },
            new Producto { Nombre = "Abrigo Largo", Descripcion = "Lana oversize", Precio = 9500, Stock = 4, ImagenUrl = "/img/abrigo.jpg" }
        );
        db.SaveChanges();
    }
}

app.Run();

// ─────────────── MODELOS Y DTOs ───────────────

public class ClienteDto
{
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Email { get; set; } = "";
}

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
}

public class Compra
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public decimal Total { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string ApellidoCliente { get; set; } = string.Empty;
    public string EmailCliente { get; set; } = string.Empty;
    public List<ItemCompra> Items { get; set; } = new();
}

public class ItemCompra
{
    public int Id { get; set; }
    public int CompraId { get; set; }
    public Compra? Compra { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public class TiendaDb : DbContext
{
    public TiendaDb(DbContextOptions<TiendaDb> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<ItemCompra> ItemsCompra => Set<ItemCompra>();
}
