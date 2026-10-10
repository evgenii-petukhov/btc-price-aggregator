using PriceAggregator.API.Services.Abstractions.Models;

namespace PriceAggregator.API.Services.Abstractions;

public interface IBitfinexRequestService
{
    Task<CandleResponse> GetPriceAsync(
        long unixTimeStart,
        long unixTimeEnd,
        CancellationToken cancellationToken);
}
