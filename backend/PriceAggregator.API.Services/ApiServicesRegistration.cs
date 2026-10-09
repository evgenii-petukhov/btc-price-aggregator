using Microsoft.Extensions.DependencyInjection;
using PriceAggregator.API.Services.Abstractions;

namespace PriceAggregator.API.Services;

public static class ApiServicesRegistration
{

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<IPriceService, PriceService>();
        return services;
    }
}
