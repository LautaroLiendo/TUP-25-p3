using cliente.Models;
using System.Net.Http.Json;

namespace cliente.Services;

public class ProductoService
{
    private readonly HttpClient _http;

    public ProductoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Producto>> ObtenerProductos(string? buscar = null)
    {
        var url = "/productos";
        if (!string.IsNullOrWhiteSpace(buscar))
            url += $"?buscar={buscar}";

        var productos = await _http.GetFromJsonAsync<List<Producto>>(url);
        return productos ?? new List<Producto>();
    }
}
