using System.Net.Http.Json;
using BlazorBasics.Web.Client.Models;

namespace BlazorBasics.Web.Client.Services;

public class LlmServiceClient(HttpClient http) : ILlmService
{
    public async Task<List<Llm>> GetLeaderboardAsync()
        => await http.GetFromJsonAsync<List<Llm>>("api/llms") ?? new();

    public async Task<int> UpvoteAsync(int llmId)
    {
        var response = await http.PostAsync($"api/llms/{llmId}/upvote", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }
}
