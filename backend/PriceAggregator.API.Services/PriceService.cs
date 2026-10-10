using PriceAggregator.API.Models.Dtos;
using PriceAggregator.API.Services.Abstractions;

namespace PriceAggregator.API.Services;

public class PriceService(
    IBitfinexRequestService bitfinexRequestService,
    IBitstampRequestService bitstampRequestService,
    IUnixTimeRangeCalculator unixTimeRangeCalculator) : IPriceService
{
    private readonly IBitfinexRequestService _bitfinexRequestService = bitfinexRequestService;
    private readonly IBitstampRequestService _bitstampRequestService = bitstampRequestService;
    private readonly IUnixTimeRangeCalculator _unixTimeRangeCalculator = unixTimeRangeCalculator;

    public async Task<PriceDto> GetPriceAsync(DateTime date, CancellationToken cancellationToken)
    {
        var (start, end) = _unixTimeRangeCalculator.GetRange(date);
        var bitstampStart = _unixTimeRangeCalculator.GetHourStartInSeconds(date);

        var bitfinexResponse = _bitfinexRequestService.GetPriceAsync(start, end, cancellationToken);
        var bitstampResponse = _bitstampRequestService.GetPriceAsync(bitstampStart, cancellationToken);

        await Task.WhenAll(bitfinexResponse, bitstampResponse);

        return new PriceDto
        {
            BitfinexPrice = bitfinexResponse.Result.PriceClose,
            BitstampPrice = bitstampResponse.Result.PriceClose,
            Date = DateTime.UnixEpoch.AddSeconds(bitstampStart)
        };
    }
}
