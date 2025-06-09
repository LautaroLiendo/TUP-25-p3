using cliente;
using cliente.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7274/") });
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<CarritoService>();

await builder.Build().RunAsync();
