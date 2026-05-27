using System.Net.Http.Json;
using BlazorBasics.Web.Client.Models;

namespace BlazorBasics.Web.Client.Services;

public class ComparisonServiceClient(HttpClient http) : IComparisonService
{
    public async Task<List<Comparison>> ListAsync()
        => await http.GetFromJsonAsync<List<Comparison>>("api/comparisons") ?? new();

    public async Task<Comparison?> GetAsync(int id)
        => await http.GetFromJsonAsync<Comparison>($"api/comparisons/{id}");

    public async Task<Comparison> SubmitAsync(Comparison comparison)
    {
        var response = await http.PostAsJsonAsync("api/comparisons", comparison);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Comparison>() ?? comparison;
    }
}
