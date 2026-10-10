using Microsoft.EntityFrameworkCore;
using PriceAggregator.Persistence.Domain;
using PriceAggregator.Persistence.Enums;
using PriceAggregator.Persistence.Repository.Abstraction;

namespace PriceAggregator.Persistence.Repository;

public class CandleRepository(PricesDbContext context) : ICandleRepository
{
    private readonly PricesDbContext _context = context;

    public Task<Candle?> GetAsync(
        long startUnixTime,
        long endUnixTime,
        SourceType sourceType,
        InstrumentType instrumentType,
        CancellationToken cancellationToken = default)
    {
        return _context.Candles.FirstOrDefaultAsync(
            c => c.StartUnixTime == startUnixTime
                && c.EndUnixTime == endUnixTime
                && c.SourceType == sourceType
                && c.InstrumentType == instrumentType,
            cancellationToken: cancellationToken);
    }

    public async Task AddAsync(
        long startUnixTime,
        long endUnixTime,
        SourceType sourceType,
        InstrumentType instrumentType,
        decimal priceOpen,
        decimal priceClose,
        decimal priceHigh,
        decimal priceLow,
        decimal volume)
    {
        await _context.Candles.AddAsync(new Candle
        {
            StartUnixTime = startUnixTime,
            EndUnixTime = endUnixTime,
            SourceType = sourceType,
            InstrumentType = instrumentType,
            PriceOpen = priceOpen,
            PriceClose = priceClose,
            PriceHigh = priceHigh,
            PriceLow = priceLow,
            Volume = volume
        });
    }
}
