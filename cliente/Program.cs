using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http;
using cliente;
using cliente.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Registramos el componente principal de la app
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configuramos HttpClient para que apunte al backend (API en el proyecto servidor)
builder.Services.AddScoped(sp =>
    new HttpClient {
        BaseAddress = new Uri("https://localhost:5001/") // Cambiá el puerto si tu servidor usa otro
    });

// Registramos los servicios que se van a encargar de consumir la API
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<CarritoService>();

await builder.Build().RunAsync();
