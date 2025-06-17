using Microsoft.EntityFrameworkCore;
using TiendaRopaApi.Models;

namespace TiendaRopaApi.Data;

public class RopaContext : DbContext
{
    public RopaContext(DbContextOptions<RopaContext> options) : base(options) { }

    public DbSet<Prenda> Prendas { get; set; }
    public DbSet<Orden> Ordenes { get; set; }
    public DbSet<DetalleOrden> DetallesOrden { get; set; }
}
