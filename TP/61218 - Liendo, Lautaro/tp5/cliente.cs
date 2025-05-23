using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


var apiBase = "http://localhost:5000";
var client = new HttpClient();
var jsonSettings = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
};

var productoNombres = new Dictionary<int, string>
{
    {1, "Martillo"},
    {2, "Taladro"},
    {3, "Destornillador"},
    {4, "Sierra"},
    {5, "Llave inglesa"},
    {6, "Cinta métrica"},
    {7, "Alicate"},
    {8, "Nivel"},
    {9, "Pistola de pegamento"},
    {10, "Lijadora"}
};

var productoPrecios = new Dictionary<int, decimal>
{
    {1, 150.50m},
    {2, 230.00m},
    {3, 99.99m},
    {4, 180.75m},
    {5, 120.00m},
    {6, 45.30m},
    {7, 78.80m},
    {8, 60.40m},
    {9, 200.00m},
    {10, 340.90m}
};

bool continuar = true;

while (continuar)
{
    Console.WriteLine("\n*** MENÚ PRINCIPAL ***");
    Console.WriteLine("1) Mostrar todos los productos");
    Console.WriteLine("2) Mostrar productos con poco stock");
    Console.WriteLine("3) Añadir stock a un producto");
    Console.WriteLine("4) Reducir stock de un producto");
    Console.WriteLine("0) Terminar");
    Console.Write("Selecciona una opción: ");

    var eleccion = Console.ReadLine();

    switch (eleccion)
    {
        case "1":
            await MostrarProductosAsync("/productos");
            break;
        case "2":
            await MostrarProductosAsync("/reposicion");
            break;
        case "3":
            await CambiarStockAsync("agregar-stock");
            break;
        case "4":
            await CambiarStockAsync("quitar-stock");
            break;
        case "0":
            continuar = false;
            break;
        default:
            Console.WriteLine("Opción no válida, intenta de nuevo.");
            break;
    }
}

async Task MostrarProductosAsync(string endpoint)
{
    try
    {
        var lista = await client.GetFromJsonAsync<List<Articulo>>($"{apiBase}{endpoint}", jsonSettings);
        if (lista == null)
        {
            Console.WriteLine("No se pudieron obtener los productos.");
            return;
        }

        var ordenados = lista.OrderBy(x => x.Id).ToList();

        Console.WriteLine("\nID  Nombre               Precio     Stock");
        foreach (var art in ordenados)
        {
            var nombreReal = productoNombres.ContainsKey(art.Id) ? productoNombres[art.Id] : art.Nombre;
            var precioReal = productoPrecios.ContainsKey(art.Id) ? productoPrecios[art.Id] : art.Precio;

            Console.WriteLine($"{art.Id,2}  {nombreReal,-20} {precioReal,9:C} {art.Stock,6}");
        }
    }
    catch
    {
        Console.WriteLine("Error al consultar los productos.");
    }
}

async Task CambiarStockAsync(string accion)
{
    Console.Write("Introduce el ID del producto: ");
    if (!int.TryParse(Console.ReadLine(), out int idProd))
    {
        Console.WriteLine("ID inválido.");
        return;
    }

    Console.Write("Introduce la cantidad: ");
    if (!int.TryParse(Console.ReadLine(), out int cantidadProd))
    {
        Console.WriteLine("Cantidad inválida.");
        return;
    }

    var uri = $"{apiBase}/{accion}?id={idProd}&cantidad={cantidadProd}";
    var resp = await client.PostAsync(uri, null);

    if (resp.IsSuccessStatusCode)
    {
        var productoActualizado = await resp.Content.ReadFromJsonAsync<Articulo>(jsonSettings);
        Console.WriteLine($"Stock actualizado para '{productoActualizado!.Nombre}': {productoActualizado.Stock}");
    }
    else
    {
        var errorMsg = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"Error: {errorMsg}");
    }
}

class Articulo
