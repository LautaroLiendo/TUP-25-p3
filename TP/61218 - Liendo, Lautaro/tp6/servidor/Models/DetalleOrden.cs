namespace TiendaRopaApi.Models;

public class DetalleOrden
{
    public int DetalleOrdenId { get; set; }
    public int PrendaId { get; set; }
    public int OrdenId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
