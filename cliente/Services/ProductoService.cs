using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using cliente.Dtos;

namespace cliente.Services
{
    public class ProductoService
    {
        private readonly HttpClient _http;

        public ProductoService(HttpClient http)
        {
            _http = http;
        }

        // Trae todos los productos o filtra por nombre
        public async Task<List<ProductoDto>> GetProductos(string? buscar = null)
        {
            string url = "productos";
            if (!string.IsNullOrWhiteSpace(buscar))
                url += $"?buscar={buscar}";

            var productos = await _http.GetFromJsonAsync<List<ProductoDto>>(url);
            return productos ?? new List<ProductoDto>();
        }
    }
}
