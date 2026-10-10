using PriceAggregator.API.Models.Dtos;
using PriceAggregator.API.Services.Abstractions;
using PriceAggregator.Persistence.Enums;

namespace PriceAggregator.API.Services;

public class PriceSummaryService(
    ICandleProvider candleProvider,
    IUnixTimeRangeCalculator unixTimeRangeCalculator) : IPriceSummaryService
{
    private readonly ICandleProvider _candleProvider = candleProvider;
    private readonly IUnixTimeRangeCalculator _unixTimeRangeCalculator = unixTimeRangeCalculator;

    public async Task<PriceSummaryDto> GetSummaryAsync(
        DateTime date,
        InstrumentType instrumentType,
        CancellationToken cancellationToken)
    {
        var (start, end) = _unixTimeRangeCalculator.GetRange(date);

        var sourceTypes = Enum.GetValues<SourceType>();

        var candles = await _candleProvider.GetCandlesAsync(instrumentType, start, end, sourceTypes, cancellationToken);

        return new PriceSummaryDto
        {
            Instrument = instrumentType.ToString().ToUpperInvariant(),
            Date = DateTime.UnixEpoch.AddSeconds(start / 1000),
            Prices = candles.Select((candle, index) => new PriceItemDto
            {
                Price = candle.PriceClose,
                SourceName = sourceTypes[index].ToString()
            }),
            IsCached = candles.All(c => c.IsCached),
        };
    }
}
