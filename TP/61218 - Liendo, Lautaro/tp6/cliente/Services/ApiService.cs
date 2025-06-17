using System.Net.Http.Json;

namespace cliente.Services
{
    public class TiendaService
    {
        private readonly HttpClient _clienteHttp;

        public event Func<Task>? CarritoActualizado;

        private string? _idCarrito;

        public TiendaService(HttpClient clienteHttp)
        {
            _clienteHttp = clienteHttp;
        }

        // Obtener mensaje y fecha desde API raíz
        public async Task<RespuestaBase> ObtenerInfoAsync()
        {
            try
            {
                var respuesta = await _clienteHttp.GetFromJsonAsync<RespuestaBase>("/");
                return respuesta ?? new RespuestaBase { Mensaje = "No se recibió respuesta", Fecha = DateTime.Now };
            }
            catch (Exception ex)
            {
                return new RespuestaBase { Mensaje = $"Error: {ex.Message}", Fecha = DateTime.Now };
            }
        }

        // Obtener prendas (productos) o buscar por término
        public async Task<List<PrendaDto>> ObtenerPrendasAsync(string filtro = "")
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return await _clienteHttp.GetFromJsonAsync<List<PrendaDto>>("/prendas") ?? new List<PrendaDto>();

            return await _clienteHttp.GetFromJsonAsync<List<PrendaDto>>($"/prendas/buscar/{filtro}") ?? new List<PrendaDto>();
        }

        // Obtener los items del carrito actual
        public async Task<List<ItemCarritoDto>> ObtenerItemsCarritoAsync()
        {
            var id = await ObtenerIdCarritoAsync();
            return await _clienteHttp.GetFromJsonAsync<List<ItemCarritoDto>>($"/carrito/{id}") ?? new List<ItemCarritoDto>();
        }

        // Agregar o actualizar cantidad en el carrito
        public async Task AgregarOActualizarCarritoAsync(int prendaId, int cantidad)
        {
            if (cantidad <= 0) return;

            var id = await ObtenerIdCarritoAsync();
            var url = $"/carrito/{id}/agregar/{prendaId}?cantidad={cantidad}";

            var respuesta = await _clienteHttp.PutAsync(url, null);
            respuesta.EnsureSuccessStatusCode();

            if (CarritoActualizado != null)
                await CarritoActualizado.Invoke();
        }

        // Quitar ítem o disminuir cantidad en carrito
        public async Task QuitarDelCarritoAsync(int prendaId, int cantidad = 0)
        {
            var id = await ObtenerIdCarritoAsync();
            var url = cantidad > 0 
                ? $"/carrito/{id}/quitar/{prendaId}?cantidad={cantidad}"
                : $"/carrito/{id}/quitar/{prendaId}";

            var respuesta = await _clienteHttp.DeleteAsync(url);
            respuesta.EnsureSuccessStatusCode();

            if (CarritoActualizado != null)
                await CarritoActualizado.Invoke();
        }

        // Confirmar compra con datos del cliente
        public async Task<RespuestaCompraDto> ConfirmarCompraAsync(ClienteInfoDto cliente)
        {
            var id = await ObtenerIdCarritoAsync();

            var contenidoJson = System.Text.Json.JsonSerializer.Serialize(cliente);
            var contenido = new StringContent(contenidoJson, System.Text.Encoding.UTF8, "application/json");

            var respuesta = await _clienteHttp.PutAsync($"/carrito/{id}/confirmar", contenido);
            respuesta.EnsureSuccessStatusCode();

            var resultado = await respuesta.Content.ReadFromJsonAsync<RespuestaCompraDto>();

            if (CarritoActualizado != null)
                await CarritoActualizado.Invoke();

            return resultado!;
        }

        // Obtener o crear nuevo carrito
        private async Task<string> ObtenerIdCarritoAsync()
        {
            if (!string.IsNullOrEmpty(_idCarrito)) return _idCarrito;

            var respuesta = await _clienteHttp.PostAsync("/carrito/nuevo", null);
            respuesta.EnsureSuccessStatusCode();

            var data = await respuesta.Content.ReadFromJsonAsync<RespuestaCarritoDto>();

            _idCarrito = data?.IdCarrito ?? Guid.NewGuid().ToString();

            return _idCarrito;
        }
    }

    // DTOs adaptados y renombrados

    public class PrendaDto
    {
        public int PrendaId { get; set; }
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Precio { get; set; }
        public int Inventario { get; set; }
        public string Imagen { get; set; } = "";
    }

    public class ItemCarritoDto
    {
        public int PrendaId { get; set; }
        public string NombrePrenda { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class ClienteInfoDto
    {
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Correo { get; set; } = "";
    }

    public class RespuestaCompraDto
    {
        public int OrdenId { get; set; }
        public decimal Total { get; set; }
    }

    public class RespuestaCarritoDto
    {
        public string IdCarrito { get; set; } = "";
    }

    public class RespuestaBase
    {
        public string Mensaje { get; set; } = "";
        public DateTime Fecha { get; set; }
    }
}
