using System.Net.Http.Json;
using cliente.Dtos;

namespace cliente.Services;

public class CarritoService
{
    private readonly HttpClient _http;
    private Guid? _carritoId;

    public CarritoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ItemCompraDto>> ObtenerCarrito()
    {
        try
        {
            if (_carritoId == null)
                await CrearCarrito();

            return await _http.GetFromJsonAsync<List<ItemCompraDto>>($"carritos/{_carritoId}") ?? new List<ItemCompraDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener el carrito: {ex.Message}");
            return new List<ItemCompraDto>();
        }
    }

    public async Task CrearCarrito()
    {
        try
        {
            var response = await _http.PostAsJsonAsync("carritos", new { });
            response.EnsureSuccessStatusCode();
            _carritoId = await response.Content.ReadFromJsonAsync<Guid>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear el carrito: {ex.Message}");
            throw;
        }
    }

    public async Task AgregarProducto(int productoId)
    {
        try
        {
            if (_carritoId == null)
                await CrearCarrito();

            var response = await _http.PutAsync($"carritos/{_carritoId}/{productoId}", null);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al agregar producto al carrito: {ex.Message}");
            throw;
        }
    }

    public async Task QuitarProducto(int productoId)
    {
        try
        {
            if (_carritoId == null)
                await CrearCarrito();

            var response = await _http.DeleteAsync($"carritos/{_carritoId}/{productoId}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al quitar producto del carrito: {ex.Message}");
            throw;
        }
    }

    public async Task VaciarCarrito()
    {
        try
        {
            if (_carritoId == null)
                await CrearCarrito();

            var response = await _http.DeleteAsync($"carritos/{_carritoId}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al vaciar el carrito: {ex.Message}");
            throw;
        }
    }

    public async Task ConfirmarCompra(dtos.ClienteDto cliente)
    {
        try
        {
            if (_carritoId == null)
                await CrearCarrito();

            var response = await _http.PutAsJsonAsync($"carritos/{_carritoId}/confirmar", cliente);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al confirmar compra: {ex.Message}");
            throw;
        }
    }
}