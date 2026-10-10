using PriceAggregator.API.Models.Dtos;
using PriceAggregator.API.Services.Abstractions;
using PriceAggregator.Persistence.Enums;

namespace PriceAggregator.API.Services;

public class PriceService(
    IRequestCacheService requestCacheService,
    IUnixTimeRangeCalculator unixTimeRangeCalculator) : IPriceService
{
    private readonly IRequestCacheService _requestCacheService = requestCacheService;
    private readonly IUnixTimeRangeCalculator _unixTimeRangeCalculator = unixTimeRangeCalculator;

    public async Task<PriceSummaryDto> GetPriceAsync(
        DateTime date,
        CancellationToken cancellationToken)
    {
        var (start, end) = _unixTimeRangeCalculator.GetRange(date);

        var sourceTypes = Enum.GetValues<SourceType>();

        var candles = await _requestCacheService.GetPricesAsync(start, end, sourceTypes, cancellationToken);

        return new PriceSummaryDto
        {
            Date = DateTime.UnixEpoch.AddSeconds(start / 1000),
            Prices = candles.Select((candle, index) => new PriceItemDto
            {
                Price = candle.PriceClose,
                SourceName = sourceTypes[index].ToString()
            })
        };
    }
}
