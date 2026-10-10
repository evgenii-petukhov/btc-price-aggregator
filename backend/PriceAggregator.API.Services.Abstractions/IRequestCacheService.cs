using PriceAggregator.API.Models.Dtos;
using PriceAggregator.Persistence.Enums;

namespace PriceAggregator.API.Services.Abstractions;

public interface IRequestCacheService
{
    Task<IReadOnlyList<CandleDto>> GetPricesAsync(
        long unixTimeStart,
        long unixTimeEnd,
        IReadOnlyCollection<SourceType> sourceTypes,
        CancellationToken cancellationToken = default);
}
