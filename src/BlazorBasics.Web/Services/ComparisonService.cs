using BlazorBasics.Web.Client.Models;
using BlazorBasics.Web.Client.Services;

namespace BlazorBasics.Web.Services;

public class ComparisonService : IComparisonService
{
    private readonly List<Comparison> _comparisons = new();
    private readonly Lock _lock = new();
    private int _nextId = 1;

    public Task<List<Comparison>> ListAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_comparisons.ToList());
        }
    }

    public Task<Comparison?> GetAsync(int id)
    {
        lock (_lock)
        {
            return Task.FromResult(_comparisons.FirstOrDefault(c => c.Id == id));
        }
    }

    public Task<Comparison> SubmitAsync(Comparison comparison)
    {
        lock (_lock)
        {
            // The form binder leaves Responses null when the form has no fields for it;
            // normalize so downstream rendering can assume the collection exists.
            comparison.Responses ??= new();
            comparison.Submitter ??= new();
            comparison.Id = _nextId++;
            _comparisons.Add(comparison);
            return Task.FromResult(comparison);
        }
    }
}
