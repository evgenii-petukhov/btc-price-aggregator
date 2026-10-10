using PriceAggregator.API.Services.Abstractions;
using PriceAggregator.API.Services.Abstractions.Models;
using PriceAggregator.API.Services.Models;
using PriceAggregator.Persistence.Enums;
using System.Globalization;
using System.Net.Http.Json;

namespace PriceAggregator.API.Services;

public class BitstampRequestService(
    IHttpClientFactory httpClientFactory) : IRequestService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public SourceType SourceType { get; } = SourceType.Bitstamp;

    public async Task<CandleResponse?> GetPriceAsync(
        InstrumentType instrumentType,
        long unixTimeStart,
        long unixTimeEnd,
        CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient(nameof(BitstampRequestService));

        var url = $"api/v2/ohlc/{GetPair(instrumentType)}/?step=3600&limit=1&start={unixTimeStart / 1000}";

        using var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<OhlcResponse>(cancellationToken);
        var candle = body?.Data?.Ohlc?.FirstOrDefault();

        if (candle == null || long.Parse(candle.Timestamp, CultureInfo.InvariantCulture) * 1000 != unixTimeStart)
        {
            return null;
        }

        return Map(candle);
    }

    private static string GetPair(InstrumentType instrumentType)
    {
        return instrumentType switch
        {
            InstrumentType.BtcUsd => "btcusd",
            InstrumentType.EthUsd => "ethusd",
            _ => throw new NotSupportedException($"Instrument '{instrumentType}' is not supported by Bitstamp.")
        };
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
