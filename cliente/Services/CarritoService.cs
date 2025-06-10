using cliente.Dtos;
using System.Net.Http.Json;

namespace cliente.Services;

public class CarritoService
{
    private readonly HttpClient _http;
    private List<ProductoDto> carrito = new();

    public CarritoService(HttpClient http)
    {
        _http = http;
    }

    public void AgregarProducto(ProductoDto producto)
    {
        carrito.Add(producto);
    }

    public void QuitarProducto(ProductoDto producto)
    {
        carrito.Remove(producto);
    }

    public List<ProductoDto> ObtenerCarrito()
    {
        return carrito;
    }

    public List<ProductoDto> ObtenerProductos()
    {
        return carrito;
    }

    public async Task ConfirmarCompra(string nombreCliente)
    {
        var datos = new
        {
            cliente = nombreCliente,
            productos = carrito
        };

        await _http.PostAsJsonAsync("carrito/confirmar", datos);
        carrito.Clear();
    }
}
