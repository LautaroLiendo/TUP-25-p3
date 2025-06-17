using System;
using System.Collections.Generic;

namespace TiendaRopaApi.Models;

public class Orden
{
    public int OrdenId { get; set; }
    public DateTime FechaOrden { get; set; }
    public decimal Total { get; set; }

    public string NombreCliente { get; set; }
    public string ApellidoCliente { get; set; }
    public string EmailCliente { get; set; }

    public List<DetalleOrden> Detalles { get; set; } = new();
}
