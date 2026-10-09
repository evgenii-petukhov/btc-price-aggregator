using PriceAggregator.API.Services.Abstractions.Models;

namespace PriceAggregator.API.Services.Abstractions;

public interface IBitfinexService
{
    Task<BitfinexResponse> GetPriceAsync(
        long unixTimeStart,
        long unixTimeEnd,
        CancellationToken cancellationToken);
}
