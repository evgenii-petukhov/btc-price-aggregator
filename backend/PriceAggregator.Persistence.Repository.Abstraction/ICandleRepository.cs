using PriceAggregator.Persistence.Domain;
using PriceAggregator.Persistence.Enums;

namespace PriceAggregator.Persistence.Repository.Abstraction;

public interface ICandleRepository
{
    Task<Candle?> GetAsync(
        long startUnixTime,
        long endUnixTime,
        SourceType sourceType,
        InstrumentType instrumentType,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        long startUnixTime,
        long endUnixTime,
        SourceType sourceType,
        InstrumentType instrumentType,
        decimal priceOpen,
        decimal priceClose,
        decimal priceHigh,
        decimal priceLow,
        decimal volume);
}
