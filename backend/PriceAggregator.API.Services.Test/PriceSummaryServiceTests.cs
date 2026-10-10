using FluentAssertions;
using Moq;
using NUnit.Framework;
using PriceAggregator.API.Models.Dtos;
using PriceAggregator.API.Services.Abstractions;
using PriceAggregator.Persistence.Enums;

namespace PriceAggregator.API.Services.Test;

[TestFixture]
public class PriceSummaryServiceTests
{
    private Mock<ICandleProvider> _candleProvider = null!;
    private Mock<IUnixTimeRangeCalculator> _rangeCalculator = null!;
    private PriceSummaryService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _candleProvider = new Mock<ICandleProvider>();
        _rangeCalculator = new Mock<IUnixTimeRangeCalculator>();
        _service = new PriceSummaryService(_candleProvider.Object, _rangeCalculator.Object);
    }

    [Test]
    public async Task GetSummaryAsync_RequestsAllSourcesForCalculatedRange()
    {
        // Arrange
        var date = new DateTime(2023, 1, 1, 0, 30, 0, DateTimeKind.Utc);
        var token = new CancellationTokenSource().Token;
        SetupCandles(
            new CandleDto { PriceClose = 1 },
            new CandleDto { PriceClose = 2 });

        // Act
        await _service.GetSummaryAsync(date, InstrumentType.BtcUsd, token);

        // Assert
        _rangeCalculator.Verify(c => c.GetRange(date), Times.Once);
        _candleProvider.Verify(p => p.GetCandlesAsync(
            InstrumentType.BtcUsd,
            1672531200000L,
            1672534799999L,
            It.Is<IReadOnlyCollection<SourceType>>(s => s.SequenceEqual(Enum.GetValues<SourceType>())),
            token), Times.Once);
    }

    [Test]
    public async Task GetSummaryAsync_MapsInstrumentDateAndPrices()
    {
        // Arrange
        SetupCandles(
            new CandleDto { PriceClose = 16531.00m },
            new CandleDto { PriceClose = 16529.50m });

        // Act
        var result = await _service.GetSummaryAsync(
            new DateTime(2023, 1, 1, 0, 30, 0, DateTimeKind.Utc),
            InstrumentType.BtcUsd,
            CancellationToken.None);

        // Assert
        result.Instrument.Should().Be("BTCUSD");
        result.Date.Should().Be(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        result.Prices.Should().BeEquivalentTo(
        [
            new PriceItemDto { SourceName = "Bitstamp", Price = 16531.00m },
            new PriceItemDto { SourceName = "Bitfinex", Price = 16529.50m }
        ], o => o.WithStrictOrdering());
    }

    [Test]
    public async Task GetSummaryAsync_AllCandlesCached_IsCachedTrue()
    {
        // Arrange
        SetupCandles(
            new CandleDto { IsCached = true },
            new CandleDto { IsCached = true });

        // Act
        var result = await _service.GetSummaryAsync(DateTime.UtcNow, InstrumentType.EthUsd, CancellationToken.None);

        // Assert
        result.IsCached.Should().BeTrue();
        result.Instrument.Should().Be("ETHUSD");
    }

    [Test]
    public async Task GetSummaryAsync_SomeCandlesNotCached_IsCachedFalse()
    {
        // Arrange
        SetupCandles(
            new CandleDto { IsCached = true },
            new CandleDto { IsCached = false });

        // Act
        var result = await _service.GetSummaryAsync(DateTime.UtcNow, InstrumentType.BtcUsd, CancellationToken.None);

        // Assert
        result.IsCached.Should().BeFalse();
    }

    [Test]
    public async Task GetSummaryAsync_ProviderThrows_PropagatesException()
    {
        // Arrange
        _rangeCalculator.Setup(c => c.GetRange(It.IsAny<DateTime>())).Returns((0L, 1L));
        _candleProvider
            .Setup(p => p.GetCandlesAsync(
                It.IsAny<InstrumentType>(), It.IsAny<long>(), It.IsAny<long>(),
                It.IsAny<IReadOnlyCollection<SourceType>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));
        var act = () => _service.GetSummaryAsync(DateTime.UtcNow, InstrumentType.BtcUsd, CancellationToken.None);

        // Act & Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    private void SetupCandles(params CandleDto[] candles)
    {
        _rangeCalculator
            .Setup(c => c.GetRange(It.IsAny<DateTime>()))
            .Returns((1672531200000L, 1672534799999L));

        _candleProvider
            .Setup(p => p.GetCandlesAsync(
                It.IsAny<InstrumentType>(),
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<IReadOnlyCollection<SourceType>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(candles);
    }
}
