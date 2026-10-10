using PriceAggregator.API.Services.Abstractions.Models;

namespace PriceAggregator.API.Services.Abstractions;

public interface IBitstampRequestService
{
    Task<Candle> GetPriceAsync(
        long unixTimeStart,
        CancellationToken cancellationToken);
}
