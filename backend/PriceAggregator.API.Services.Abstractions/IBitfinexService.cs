using PriceAggregator.API.Services.Abstractions.Models;

namespace PriceAggregator.API.Services.Abstractions;

public interface IBitfinexService
{
    Task<Candle> GetPriceAsync(
        long unixTimeStart,
        long unixTimeEnd,
        CancellationToken cancellationToken);
}
