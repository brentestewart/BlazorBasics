using BlazorBasics.Web.Client.Models;
using BlazorBasics.Web.Client.Services;

namespace BlazorBasics.Web.Services;

public class LlmService : ILlmService
{
    private readonly List<Llm> _llms = new()
    {
        new Llm { Id = 1, Name = "Claude",  Tagline = "Anthropic", Votes = 0 },
        new Llm { Id = 2, Name = "GPT",     Tagline = "OpenAI",    Votes = 0 },
        new Llm { Id = 3, Name = "Gemini",  Tagline = "Google",    Votes = 0 },
        new Llm { Id = 4, Name = "Llama",   Tagline = "Meta",      Votes = 0 },
    };

    private readonly Lock _lock = new();

    public Task<List<Llm>> GetLeaderboardAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_llms.OrderByDescending(l => l.Votes).ToList());
        }
    }

    public Task<int> UpvoteAsync(int llmId)
    {
        lock (_lock)
        {
            var llm = _llms.FirstOrDefault(l => l.Id == llmId);
            if (llm is null) return Task.FromResult(0);
            llm.Votes++;
            return Task.FromResult(llm.Votes);
        }
    }
}
