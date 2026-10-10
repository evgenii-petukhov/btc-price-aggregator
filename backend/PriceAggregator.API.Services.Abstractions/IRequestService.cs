using PriceAggregator.API.Services.Abstractions.Models;
using PriceAggregator.Persistence.Enums;

namespace PriceAggregator.API.Services.Abstractions;

public interface IRequestService
{
    SourceType SourceType { get; }

    Task<CandleResponse> GetPriceAsync(
        InstrumentType instrumentType,
        long unixTimeStart,
        long unixTimeEnd,
        CancellationToken cancellationToken);
}
