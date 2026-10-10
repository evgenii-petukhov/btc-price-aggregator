using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PriceAggregator.API.Services.Abstractions;
using PriceAggregator.Persistence.Repository;

namespace PriceAggregator.API.Services;

public static class ApiServicesRegistration
{

    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IPriceService, PriceService>();
        services.AddScoped<IRequestService, BitfinexRequestService>();
        services.AddScoped<IRequestService, BitstampRequestService>();
        services.AddScoped<IRequestServiceResolver, RequestServiceResolver>();
        services.AddScoped<IRequestCacheService, RequestCacheService>();
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
        services.AddPersistenceRepositoryServices(configuration);
        return services;
    }
}
