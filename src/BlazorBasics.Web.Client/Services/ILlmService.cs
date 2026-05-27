using BlazorBasics.Web.Client.Models;

namespace BlazorBasics.Web.Client.Services;

public interface ILlmService
{
    Task<List<Llm>> GetLeaderboardAsync();
    Task<int> UpvoteAsync(int llmId);
}
