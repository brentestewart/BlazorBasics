using BlazorBasics.Web.Client.Models;

namespace BlazorBasics.Web.Client.Services;

public interface IComparisonService
{
    Task<List<Comparison>> ListAsync();
    Task<Comparison?> GetAsync(int id);
    Task<Comparison> SubmitAsync(Comparison comparison);
}
