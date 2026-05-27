using BlazorBasics.Web.Client;
using BlazorBasics.Web.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IComparisonService, ComparisonServiceClient>();
builder.Services.AddClientValidation();

await builder.Build().RunAsync();
