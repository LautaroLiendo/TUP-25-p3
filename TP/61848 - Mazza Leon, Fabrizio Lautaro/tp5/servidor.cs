#r "sdk:Microsoft.NET.Sdk.Web"
#r "nuget: Microsoft.EntityFrameworkCore, 9.0.4"
#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 9.0.4"

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder();
builder.Services.AddDbContext<AppDb>(opt =>
    opt.UseSqlite("Data Source=./tienda.db"));
builder.Services.Configure<JsonOptions>(opt =>
    opt.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

var app = builder.Build();


app.MapGet("/productos", async (AppDb db) =>
    await db.Productos.ToListAsync());


app.MapGet("/productos/reponer", async (AppDb db) =>
    await db.Productos.Where(p => p.Stock < 3).ToListAsync());


app.MapPost("/productos/agregar/{id}/{cantidad}", async (int id, int cantidad, AppDb db) =>
{
    var producto = await db.Productos.FindAsync(id);
    if (producto is null) return Results.NotFound();

    producto.Stock += cantidad;
    await db.SaveChangesAsync();
    return Results.Ok(producto);
});


app.MapPost("/productos/quitar/{id}/{cantidad}", async (int id, int cantidad, AppDb db) =>
{
    var producto = await db.Productos.FindAsync(id);
    if (producto is null) return Results.NotFound();
    if (producto.Stock < cantidad)
        return Results.BadRequest("El stock no puede quedar negativo.");

    producto.Stock -= cantidad;
    await db.SaveChangesAsync();
    return Results.Ok(producto);
});


var db = app.Services.GetRequiredService<AppDb>();
db.Database.EnsureCreated();

if (!db.Productos.Any())
{
    var productos = Enumerable.Range(1, 10).Select(i => new Producto
    {
        Nombre = $"Producto {i}",
        Precio = 100 + i * 10,
        Stock = 10
    });
    db.Productos.AddRange(productos);
    db.SaveChanges();
}

app.Run("http://localhost:5000");

class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }
    public DbSet<Producto> Productos => Set<Producto>();
}

class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}