using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using cliente.Dtos;
#nullable enable

namespace cliente.Services
{
    public class ProductoService
    {
        private readonly HttpClient _http;

        public ProductoService(HttpClient http) => _http = http;

        public async Task<List<ProductoDto>> ObtenerProductos(string? buscar = null)
        {
            var url = "productos" + (string.IsNullOrWhiteSpace(buscar) ? "" : $"?buscar={buscar}");
            return await _http.GetFromJsonAsync<List<ProductoDto>>(url)
                   ?? new List<ProductoDto>();
        }
    }
}
