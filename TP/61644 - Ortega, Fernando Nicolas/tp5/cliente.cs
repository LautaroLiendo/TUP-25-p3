using System;                     // Console, etc.
using System.Linq;                // OrderBy
using System.Net.Http;            // HttpClient, StringContent ✔
using System.Text.Json;           // JsonSerializer, JsonNamingPolicy
using System.Threading.Tasks;     // Task

var baseUrl = "http://localhost:5000";
var http = new HttpClient();
var jsonOpt = new JsonSerializerOptions {
    PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
};

app.MapGet("/productos", async (AppDb db) =>
    await db.Productos.ToListAsync()
);
app.MapGet("/productos/reponer", async (AppDb db) =>
    await db.Productos.Where(p => p.Stock < 3).ToListAsync()
);
app.MapPost("/productos/{id}/agregar", async (int id, int cantidad, AppDb db) => {
    var prod = await db.Productos.FindAsync(id);
    if (prod is null) return Results.NotFound();
    prod.Stock += cantidad;
    await db.SaveChangesAsync();
    return Results.Ok(prod);
});
app.MapPost("/productos/{id}/quitar", async (int id, int cantidad, AppDb db) => {
    var prod = await db.Productos.FindAsync(id);
    if (prod is null) return Results.NotFound();
    if (prod.Stock < cantidad) return Results.BadRequest("Stock insuficiente");
    prod.Stock -= cantidad;
    await db.SaveChangesAsync();
    return Results.Ok(prod);
});
var db = app.Services.GetRequiredService<AppDb>();
db.Database.EnsureCreated();
if (!db.Productos.Any()) {
    db.Productos.AddRange(
        Enumerable.Range(1, 10).Select(i =>
            new Producto {
                Nombre = $"Producto {i}",
                Precio = i * 10,
                Stock = 10
            }
        )
    );
    db.SaveChanges();
}
app.Run("http://localhost:5000");
class AppDb : DbContext {
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }
    public DbSet<Producto> Productos => Set<Producto>();
}
class Producto {
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}