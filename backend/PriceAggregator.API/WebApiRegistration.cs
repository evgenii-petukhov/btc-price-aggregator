using PriceAggregator.API.Services;
using System.Text.Json.Serialization;

namespace PriceAggregator.API;

public static class WebApiRegistration
{
    public static IServiceCollection AddWebApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddCors(options =>
        {
            options.AddPolicy("all", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddSwaggerGen();

        services.AddApiServices(configuration);

        return services;
    }
}
