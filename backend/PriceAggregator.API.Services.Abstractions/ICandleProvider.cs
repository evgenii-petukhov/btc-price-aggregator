using PriceAggregator.API.Models.Dtos;
using PriceAggregator.Persistence.Enums;

namespace PriceAggregator.API.Services.Abstractions;

public interface ICandleProvider
{
    Task<IReadOnlyList<CandleDto>> GetCandlesAsync(
        long unixTimeStart,
        long unixTimeEnd,
        IReadOnlyCollection<SourceType> sourceTypes,
        CancellationToken cancellationToken = default);
}
