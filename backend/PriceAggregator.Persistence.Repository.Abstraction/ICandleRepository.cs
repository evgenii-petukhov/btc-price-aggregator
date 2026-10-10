using PriceAggregator.Persistence.Domain;
using PriceAggregator.Persistence.Enums;

namespace PriceAggregator.Persistence.Repository.Abstraction;

public interface ICandleRepository
{
    Task<Candle?> GetAsync(
        long startUnixTime,
        long endUnixTime,
        SourceTypes sourceType,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        long startUnixTime,
        long endUnixTime,
        SourceTypes sourceType,
        decimal priceOpen,
        decimal priceClose,
        decimal priceHigh,
        decimal priceLow,
        decimal volume);
}
