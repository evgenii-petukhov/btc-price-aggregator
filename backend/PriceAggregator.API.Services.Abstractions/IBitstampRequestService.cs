using PriceAggregator.API.Services.Abstractions.Models;

namespace PriceAggregator.API.Services.Abstractions;

public interface IBitstampRequestService
{
    Task<CandleResponse> GetPriceAsync(
        long unixTimeStart,
        CancellationToken cancellationToken);
}
