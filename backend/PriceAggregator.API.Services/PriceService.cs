using PriceAggregator.API.Models.Dtos;
using PriceAggregator.API.Services.Abstractions;

namespace PriceAggregator.API.Services;

public class PriceService(
    IBitfinexService bitfinexService,
    IUnixTimeRangeCalculator unixTimeRangeCalculator) : IPriceService
{
    private readonly IBitfinexService _bitfinexService = bitfinexService;
    private readonly IUnixTimeRangeCalculator _unixTimeRangeCalculator = unixTimeRangeCalculator;

    public async Task<PriceDto> GetPriceAsync(DateTime date, CancellationToken cancellationToken)
    {
        var (start, end) = _unixTimeRangeCalculator.GetRange(date);
        var response = await _bitfinexService.GetPriceAsync(start, end, cancellationToken);
        return new PriceDto();
    }
}
