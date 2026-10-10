using PriceAggregator.API.Models.Dtos;
using PriceAggregator.API.Services.Abstractions;

namespace PriceAggregator.API.Services;

public class PriceService(
    IBitfinexRequestService bitfinexRequestService,
    IBitstampService bitstampService,
    IUnixTimeRangeCalculator unixTimeRangeCalculator) : IPriceService
{
    private readonly IBitfinexRequestService _bitfinexRequestService = bitfinexRequestService;
    private readonly IBitstampService _bitstampService = bitstampService;
    private readonly IUnixTimeRangeCalculator _unixTimeRangeCalculator = unixTimeRangeCalculator;

    public async Task<PriceDto> GetPriceAsync(DateTime date, CancellationToken cancellationToken)
    {
        var (start, end) = _unixTimeRangeCalculator.GetRange(date);

        var bitfinexResponse = _bitfinexRequestService.GetPriceAsync(start, end, cancellationToken);
        var bitstampResponse = _bitstampService.GetPriceAsync(start, end, cancellationToken);

        await Task.WhenAll(bitfinexResponse, bitstampResponse);

        return new PriceDto
        {
            BitfinexPrice = bitfinexResponse.Result.PriceClose,
            BitstampPrice = bitstampResponse.Result.PriceClose,
            Date = DateTime.UnixEpoch.AddSeconds(start / 1000)
        };
    }
}
