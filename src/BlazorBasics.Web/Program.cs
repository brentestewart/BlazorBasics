using BlazorBasics.Web.Client;
using BlazorBasics.Web.Client.Models;
using BlazorBasics.Web.Client.Services;
using BlazorBasics.Web.Components;
using BlazorBasics.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddValidation();
builder.Services.AddClientValidation();

builder.Services.AddSingleton<ILlmService, LlmService>();
builder.Services.AddSingleton<IComparisonService, ComparisonService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapGet("/api/llms", (ILlmService svc) => svc.GetLeaderboardAsync());
app.MapPost("/api/llms/{id:int}/upvote", (int id, ILlmService svc) => svc.UpvoteAsync(id));
app.MapGet("/api/comparisons", (IComparisonService svc) => svc.ListAsync());
app.MapGet("/api/comparisons/{id:int}", async (int id, IComparisonService svc) =>
{
    var c = await svc.GetAsync(id);
    return c is null ? Results.NotFound() : Results.Ok(c);
});
app.MapPost("/api/comparisons", (Comparison c, IComparisonService svc) => svc.SubmitAsync(c));

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorBasics.Web.Client._Imports).Assembly);

app.Run();
