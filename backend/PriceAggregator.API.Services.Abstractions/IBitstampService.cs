using PriceAggregator.API.Models.Dtos;

namespace PriceAggregator.API.Services.Abstractions;

public interface IBitstampService
{
    Task<CandleDto?> GetPriceAsync(
        long unixTimeStart,
        long unixTimeEnd,
        CancellationToken cancellationToken = default);
}
