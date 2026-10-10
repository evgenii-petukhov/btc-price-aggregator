using Microsoft.Extensions.DependencyInjection;
using PriceAggregator.API.Services.Abstractions;

namespace PriceAggregator.API.Services;

public static class ApiServicesRegistration
{

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<IPriceService, PriceService>();
        services.AddScoped<IBitfinexRequestService, BitfinexRequestService>();
        services.AddScoped<IBitstampRequestService, BitstampRequestService>();
        services.AddSingleton<IUnixTimeRangeCalculator, UnixTimeRangeCalculator>();
        services.AddHttpClient(
            nameof(BitstampRequestService),
            client =>
            {
                client.BaseAddress = new Uri("https://www.bitstamp.net/");
            });
        services.AddHttpClient(
            nameof(BitfinexRequestService),
            client =>
            {
                client.BaseAddress = new Uri("https://api-pub.bitfinex.com/");
            });
        return services;
    }
}
