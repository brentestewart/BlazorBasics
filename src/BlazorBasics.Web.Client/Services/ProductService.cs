using BlazorBasics.Web.Client.Models;

namespace BlazorBasics.Web.Client.Services;

public class ProductService
{
    public async Task<Product> GetAsync(int id, string fetchedFrom)
    {
        await Task.Delay(1500);
        return new Product(id, $"Acme Widget #{id}", 19.99m + id * 0.5m, fetchedFrom);
    }
}
