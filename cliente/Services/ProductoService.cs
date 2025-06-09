using System.Net.Http.Json;
using cliente.Dtos;

namespace cliente.Services
{
    public class ProductoService
    {
        private readonly HttpClient http;

        public ProductoService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<List<ProductoDto>> ObtenerProductos()
        {
            var productos = await http.GetFromJsonAsync<List<ProductoDto>>("productos");
            return productos ?? new List<ProductoDto>();
        }
    }
}
