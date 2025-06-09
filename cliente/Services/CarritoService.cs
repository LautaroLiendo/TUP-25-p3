using System.Net.Http.Json;
using cliente.Dtos;
using cliente.Models;

namespace cliente.Services
{
    public class CarritoService
    {
        private readonly HttpClient http;

        public CarritoService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<List<ProductoCarritoDto>> ObtenerCarrito()
        {
            var response = await http.GetFromJsonAsync<List<ProductoCarritoDto>>("api/carrito");
            return response ?? new List<ProductoCarritoDto>();
        }

        public async Task ConfirmarCompra(string nombreCliente)
        {
            var clienteDto = new ClienteDto { Nombre = nombreCliente };
            var response = await http.PostAsJsonAsync("api/carrito/confirmar", clienteDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task AgregarProducto(int id)
        {
            var response = await http.PostAsync($"api/carrito/{id}", null);
            response.EnsureSuccessStatusCode();
        }
    }
}
