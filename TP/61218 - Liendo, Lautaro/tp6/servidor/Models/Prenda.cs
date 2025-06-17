namespace TiendaRopaApi.Models;

public class Prenda
{
    public int PrendaId { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Inventario { get; set; }
    public string Imagen { get; set; }
}
