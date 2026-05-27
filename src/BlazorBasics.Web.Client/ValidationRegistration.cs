using Microsoft.Extensions.DependencyInjection;

namespace BlazorBasics.Web.Client;

public static class ValidationRegistration
{
    // Defined in the .Client assembly so the source-generated validators
    // for models declared here register correctly when called from the host.
    public static IServiceCollection AddClientValidation(this IServiceCollection services)
    {
        services.AddValidation();
        return services;
    }
}
