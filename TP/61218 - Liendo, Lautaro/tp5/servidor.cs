#r "sdk:Microsoft.NET.Sdk.Web"
#r "nuget: SQLitePCLRaw.bundle_e_sqlite3"

#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 7.0.11"
#r "nuget: Microsoft.EntityFrameworkCore, 7.0.11"

using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder();
builder.Services.AddDbContext<TiendaDbContext>(options =>
    options.UseSqlite("Data Source=./tienda.db"));

builder.Services.Configure<JsonOptions>(opt =>
{
    opt.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();


app.MapGet("/productos", async (TiendaDbContext context) =>
    await context.Productos.OrderBy(p => p.Nombre).ToListAsync());

app.MapGet("/reposicion", async (TiendaDbContext context) =>
    await context.Productos.Where(p => p.Stock < 3).ToListAsync());

app.MapPost("/agregar-stock", async (TiendaDbContext context, int id, int cantidad) =>
{
    var producto = await context.Productos.FindAsync(id);
    if (producto == null) return Results.NotFound("Producto no hallado");

    producto.Stock += cantidad;
    await context.SaveChangesAsync();
    return Results.Ok(producto);
});

app.MapPost("/quitar-stock", async (TiendaDbContext context, int id, int cantidad) =>
{
    var producto = await context.Productos.FindAsync(id);
    if (producto == null) return Results.NotFound("Producto no hallado");

    if (producto.Stock < cantidad)
        return Results.BadRequest("No hay suficiente stock");

    producto.Stock -= cantidad;
    await context.SaveChangesAsync();
    return Results.Ok(producto);
});


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TiendaDbContext>();
    context.Database.EnsureCreated();

    if (!context.Productos.Any())
    {
        var productosIniciales = Enumerable.Range(1, 10).Select(i =>
            new ProductoEntity { Nombre = $"Producto {i}", Precio = 100 + i, Stock = 10 });
        context.Productos.AddRange(productosIniciales);
        context.SaveChanges();
    }
}

app.Run("http://localhost:5000");


class TiendaDbContext : DbContext
{
    public TiendaDbContext(DbContextOptions<TiendaDbContext> opts) : base(opts) { }
    public DbSet<ProductoEntity> Productos => Set<ProductoEntity>();
}


class ProductoEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}
