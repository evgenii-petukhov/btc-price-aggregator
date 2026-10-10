using PriceAggregator.API.Services.Abstractions;
using PriceAggregator.API.Services.Abstractions.Models;
using PriceAggregator.Persistence.Enums;
using System.Net.Http.Json;
using System.Text.Json;

namespace PriceAggregator.API.Services;

public class BitfinexRequestService(
    IHttpClientFactory httpClientFactory) : IRequestService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public SourceType SourceType { get; } = SourceType.Bitfinex;

    public async Task<CandleResponse?> GetPriceAsync(
        InstrumentType instrumentType,
        long unixTimeStart,
        long unixTimeEnd,
        CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient(nameof(BitfinexRequestService));

        var url = $"v2/candles/trade:1h:{GetSymbol(instrumentType)}/hist?start={unixTimeStart}&end={unixTimeEnd}&limit=1&sort=1";

        using var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var candles = await response.Content.ReadFromJsonAsync<JsonElement[][]>(cancellationToken);
        if (candles == null || candles.Length == 0)
        {
            return null;
        }

        return Map(candles[0]);
    }

    private static string GetSymbol(InstrumentType instrumentType)
    {
        return instrumentType switch
        {
            InstrumentType.BtcUsd => "tBTCUSD",
            InstrumentType.EthUsd => "tETHUSD",
            _ => throw new NotSupportedException($"Instrument '{instrumentType}' is not supported by Bitfinex.")
        };
    }

    private static CandleResponse Map(JsonElement[] row)
    {
        return new CandleResponse
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
