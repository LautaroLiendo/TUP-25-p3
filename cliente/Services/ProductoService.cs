using cliente.Dtos;
using System.Net.Http.Json;

namespace cliente.Services;

public class ProductoService
{
    private readonly HttpClient _http;

    public ProductoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ProductoDto>> ObtenerProductos()
    {
        var resultado = await _http.GetFromJsonAsync<List<ProductoDto>>("productos");
        return resultado ?? new();
    }
}
