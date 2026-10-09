using Microsoft.Extensions.DependencyInjection;
using PriceAggregator.API.Services.Abstractions;

namespace PriceAggregator.API.Services;

public static class ApiServicesRegistration
{

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<IPriceService, PriceService>();
        services.AddScoped<IBitfinexService, BitfinexService>();
        services.AddSingleton<IUnixTimeRangeCalculator, UnixTimeRangeCalculator>();
        services.AddHttpClient(
            nameof(BitfinexService),
            client =>
            {
                client.BaseAddress = new Uri("https://api-pub.bitfinex.com/");
            });
        return services;
    }
}
