using PriceAggregator.API.Models.Dtos;
using PriceAggregator.API.Services.Abstractions;

namespace PriceAggregator.API.Services;

public class PriceService(
    IBitfinexService bitfinexService,
    IBitstampService bitstampService,
    IUnixTimeRangeCalculator unixTimeRangeCalculator) : IPriceService
{
    private readonly IBitfinexService _bitfinexService = bitfinexService;
    private readonly IBitstampService _bitstampService = bitstampService;
    private readonly IUnixTimeRangeCalculator _unixTimeRangeCalculator = unixTimeRangeCalculator;

    public async Task<PriceDto> GetPriceAsync(DateTime date, CancellationToken cancellationToken)
    {
        var (start, end) = _unixTimeRangeCalculator.GetRange(date);
        var bitstampStart = _unixTimeRangeCalculator.GetDayStartInSeconds(date);

        var bitfinexResponse = await _bitfinexService.GetPriceAsync(start, end, cancellationToken);
        var bitstampResponse = await _bitstampService.GetPriceAsync(bitstampStart, cancellationToken);

        return new PriceDto();
    }
}
