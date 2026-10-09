using PriceAggregator.API.Services.Abstractions;
using PriceAggregator.API.Services.Abstractions.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace PriceAggregator.API.Services;

public class BitfinexService(
    IHttpClientFactory httpClientFactory) : IBitfinexService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<Candle> GetPriceAsync(
        long unixTimeStart,
        long unixTimeEnd,
        CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient(nameof(BitfinexService));

        var url = $"v2/candles/trade:1h:tBTCUSD/hist?start={unixTimeStart}&end={unixTimeEnd}&limit=1&sort=1";

        using var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var candles = await response.Content.ReadFromJsonAsync<JsonElement[][]>(cancellationToken);
        if (candles == null || candles.Length == 0)
        {
            throw new InvalidOperationException("No candle data");
        }

        return Map(candles[0]);
    }

    private static Candle Map(JsonElement[] row)
    {
        return new Candle
        {
            UnitTimeStart = row[0].GetInt64(),
            PriceOpen = row[1].GetDecimal(),
            PriceClose = row[2].GetDecimal(),
            PriceHigh = row[3].GetDecimal(),
            PriceLow = row[4].GetDecimal(),
            Volume = row[5].GetDecimal()
        };
    }
}
