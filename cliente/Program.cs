using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http;
using cliente.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Apunta al backend que corre en https://localhost:5001/
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri("https://localhost:5001/") });

// Tus servicios para consumir la API
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<CarritoService>();

await builder.Build().RunAsync();
