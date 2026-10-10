using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PriceAggregator.Persistence.Repository.Abstraction;

namespace PriceAggregator.Persistence.Repository;

public static class PersistenceRepositoryRegistration
{
    public static IServiceCollection AddPersistenceRepositoryServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICandleRepository, CandleRepository>();
        services.AddPersistence(configuration);

        return services;
    }
}
