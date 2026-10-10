using PriceAggregator.API.Services.Abstractions;
using PriceAggregator.API.Services.Abstractions.Models;
using PriceAggregator.API.Services.Models;
using System.Globalization;
using System.Net.Http.Json;

namespace PriceAggregator.API.Services;

public class BitstampRequestService(
    IHttpClientFactory httpClientFactory) : IBitstampRequestService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<CandleResponse> GetPriceAsync(
        long unixTimeStart,
        CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient(nameof(BitstampRequestService));

        var url = $"api/v2/ohlc/btcusd/?step=3600&limit=1&start={unixTimeStart / 1000}";

        using var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<OhlcResponse>(cancellationToken);
        var candle = body?.Data?.Ohlc?.FirstOrDefault();
        if (candle == null)
        {
            throw new InvalidOperationException("No candle data");
        }

        return Map(candle);
    }

    private static CandleResponse Map(OhlcItem item)
    {
        return new CandleResponse
        {
            UnitTimeStart = long.Parse(item.Timestamp, CultureInfo.InvariantCulture),
            PriceOpen = decimal.Parse(item.Open, CultureInfo.InvariantCulture),
            PriceClose = decimal.Parse(item.Close, CultureInfo.InvariantCulture),
            PriceHigh = decimal.Parse(item.High, CultureInfo.InvariantCulture),
            PriceLow = decimal.Parse(item.Low, CultureInfo.InvariantCulture),
            Volume = decimal.Parse(item.Volume, CultureInfo.InvariantCulture)
        };
    }
}
