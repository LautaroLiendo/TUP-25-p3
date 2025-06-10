using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using cliente.dtos;

namespace cliente.Services
{
    public class CarritoService
    {
        private readonly HttpClient _http;
        private Guid? carritoId;

        public CarritoService(HttpClient http) => _http = http;

        private async Task AsegurarCarrito()
        {
            if (carritoId != null) return;
            var res = await _http.PostAsJsonAsync("carritos", new { });
            carritoId = await res.Content.ReadFromJsonAsync<Guid>();
        }

        public async Task AgregarProducto(int productoId)
        {
            await AsegurarCarrito();
            await _http.PutAsync($"carritos/{carritoId}/{productoId}", null);
        }

        public async Task QuitarProducto(int productoId)
        {
            if (carritoId == null) return;
            await _http.DeleteAsync($"carritos/{carritoId}/{productoId}");
        }

        public async Task<List<ItemCompraDto>> ObtenerCarrito()
        {
            await AsegurarCarrito();
            return await _http.GetFromJsonAsync<List<ItemCompraDto>>($"carritos/{carritoId}")
                   ?? new List<ItemCompraDto>();
        }

        public async Task VaciarCarrito()
        {
            if (carritoId == null) return;
            await _http.DeleteAsync($"carritos/{carritoId}");
        }

        public async Task<CompraDto> ConfirmarCompra(ClienteDto cliente)
        {
            await AsegurarCarrito();
            var res = await _http.PutAsJsonAsync($"carritos/{carritoId}/confirmar", cliente);
            carritoId = null;
            return await res.Content.ReadFromJsonAsync<CompraDto>();
        }
    }
}
