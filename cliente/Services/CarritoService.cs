using cliente.Models;
using cliente.Dtos;
using System.Net.Http.Json;

namespace cliente.Services;

public class CarritoService
{
    private readonly HttpClient _http;
    private Guid? _carritoId;

    public CarritoService(HttpClient http)
    {
        _http = http;
    }

    public async Task CrearCarrito()
    {
        var res = await _http.PostAsync("/carritos", null);
        if (res.IsSuccessStatusCode)
            _carritoId = await res.Content.ReadFromJsonAsync<Guid>();
    }

    public async Task<List<ItemCompra>> ObtenerCarrito()
    {
        if (_carritoId == null)
            return new();

        var items = await _http.GetFromJsonAsync<List<ItemCompra>>($"/carritos/{_carritoId}");
        return items ?? new();
    }

    public async Task AgregarProducto(int productoId)
    {
        if (_carritoId == null) return;
        await _http.PutAsync($"/carritos/{_carritoId}/{productoId}", null);
    }

    public async Task QuitarProducto(int productoId)
    {
        if (_carritoId == null) return;
        await _http.DeleteAsync($"/carritos/{_carritoId}/{productoId}");
    }

    public async Task ConfirmarCompra(ClienteDto cliente)
    {
        if (_carritoId == null) return;
        await _http.PutAsJsonAsync($"/carritos/{_carritoId}/confirmar", cliente);
    }
}
