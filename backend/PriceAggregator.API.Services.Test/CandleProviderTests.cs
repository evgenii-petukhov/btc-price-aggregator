using FluentAssertions;
using Moq;
using NUnit.Framework;
using PriceAggregator.API.Services.Abstractions;
using PriceAggregator.API.Services.Abstractions.Models;
using PriceAggregator.Persistence.Domain;
using PriceAggregator.Persistence.Enums;
using PriceAggregator.Persistence.Repository.Abstraction;

namespace PriceAggregator.API.Services.Test;

[TestFixture]
public class CandleProviderTests
{
    private const long Start = 1672531200000;
    private const long End = 1672534799999;
    private static readonly SourceType[] Sources = [SourceType.Bitstamp, SourceType.Bitfinex];

    private Mock<IRequestServiceResolver> _resolver = null!;
    private Mock<ICandleRepository> _repository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private CandleProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        _resolver = new Mock<IRequestServiceResolver>();
        _repository = new Mock<ICandleRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _provider = new CandleProvider(_resolver.Object, _repository.Object, _unitOfWork.Object);
    }

    [Test]
    public async Task GetCandlesAsync_AllStored_ReturnsCachedWithoutCallingExternalOrSaving()
    {
        // Arrange
        SetupStored(SourceType.Bitstamp, Stored(100));
        SetupStored(SourceType.Bitfinex, Stored(200));

        // Act
        var result = await _provider.GetCandlesAsync(InstrumentType.BtcUsd, Start, End, Sources);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.IsCached && c.UnitTimeStart == Start);
        result.Select(c => c.PriceClose).Should().Equal(100m, 200m);
        result[0].PriceOpen.Should().Be(99);
        result[0].PriceHigh.Should().Be(101);
        result[0].PriceLow.Should().Be(98);
        result[0].Volume.Should().Be(10);
        _resolver.Verify(r => r.Resolve(It.IsAny<SourceType>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetCandlesAsync_NoneStored_FetchesAllStoresAndSavesOnce()
    {
        // Arrange
        SetupStored(SourceType.Bitstamp, null);
        SetupStored(SourceType.Bitfinex, null);
        SetupRemote(SourceType.Bitstamp, Remote(100));
        SetupRemote(SourceType.Bitfinex, Remote(200));

        // Act
        var result = await _provider.GetCandlesAsync(InstrumentType.BtcUsd, Start, End, Sources);

        // Assert
        result.Select(c => c.PriceClose).Should().Equal(100m, 200m);
        result.Should().OnlyContain(c => !c.IsCached && c.UnitTimeStart == Start);
        _repository.Verify(r => r.AddAsync(Start, End, SourceType.Bitstamp, InstrumentType.BtcUsd, 99, 100, 101, 98, 20), Times.Once);
        _repository.Verify(r => r.AddAsync(Start, End, SourceType.Bitfinex, InstrumentType.BtcUsd, 199, 200, 201, 198, 20), Times.Once);
        _unitOfWork.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetCandlesAsync_PartiallyStored_FetchesOnlyMissingAndKeepsSourceOrder()
    {
        // Arrange
        SetupStored(SourceType.Bitstamp, Stored(100));
        SetupStored(SourceType.Bitfinex, null);
        SetupRemote(SourceType.Bitfinex, Remote(200));

        // Act
        var result = await _provider.GetCandlesAsync(InstrumentType.BtcUsd, Start, End, Sources);

        // Assert
        result.Select(c => c.PriceClose).Should().Equal(100m, 200m);
        result.Select(c => c.IsCached).Should().Equal(true, false);
        _resolver.Verify(r => r.Resolve(SourceType.Bitstamp), Times.Never);
        _repository.Verify(r => r.AddAsync(
            It.IsAny<long>(), It.IsAny<long>(), SourceType.Bitstamp, It.IsAny<InstrumentType>(),
            It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()),
            Times.Never);
        _unitOfWork.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetCandlesAsync_SourceHasNoData_OmitsItAndDoesNotStoreIt()
    {
        // Arrange
        SetupStored(SourceType.Bitstamp, null);
        SetupStored(SourceType.Bitfinex, null);
        SetupRemote(SourceType.Bitstamp, null);
        SetupRemote(SourceType.Bitfinex, Remote(200));

        // Act
        var result = await _provider.GetCandlesAsync(InstrumentType.BtcUsd, Start, End, Sources);

        // Assert
        result.Should().ContainSingle().Which.SourceType.Should().Be(SourceType.Bitfinex);
        _repository.Verify(r => r.AddAsync(
            It.IsAny<long>(), It.IsAny<long>(), SourceType.Bitstamp, It.IsAny<InstrumentType>(),
            It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()),
            Times.Never);
    }

    [Test]
    public async Task GetCandlesAsync_NoSourceHasData_ReturnsEmpty()
    {
        // Arrange
        SetupStored(SourceType.Bitstamp, null);
        SetupStored(SourceType.Bitfinex, null);
        SetupRemote(SourceType.Bitstamp, null);
        SetupRemote(SourceType.Bitfinex, null);

        // Act
        var result = await _provider.GetCandlesAsync(InstrumentType.BtcUsd, Start, End, Sources);

        // Assert
        result.Should().BeEmpty();
        VerifyNothingAdded();
    }

    [Test]
    public async Task GetCandlesAsync_OrderFollowsRequestedSourcesNotStorage()
    {
        // Arrange
        SetupStored(SourceType.Bitstamp, Stored(100));
        SetupStored(SourceType.Bitfinex, Stored(200));

        // Act
        var result = await _provider.GetCandlesAsync(
            InstrumentType.BtcUsd, Start, End, [SourceType.Bitfinex, SourceType.Bitstamp]);

        // Assert
        result.Select(c => c.PriceClose).Should().Equal(200m, 100m);
    }

    [Test]
    public async Task GetCandlesAsync_NoRequestServiceRegistered_Throws()
    {
        // Arrange
        SetupStored(SourceType.Bitstamp, null);
        _resolver.Setup(r => r.Resolve(SourceType.Bitstamp)).Returns((IRequestService?)null);
        var act = () => _provider.GetCandlesAsync(InstrumentType.BtcUsd, Start, End, [SourceType.Bitstamp]);

        // Act & Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Bitstamp*");
        _unitOfWork.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetCandlesAsync_ExternalCallFails_PropagatesAndDoesNotSave()
    {
        // Arrange
        SetupStored(SourceType.Bitstamp, null);
        var service = new Mock<IRequestService>();
        service
            .Setup(s => s.GetPriceAsync(It.IsAny<InstrumentType>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("down"));
        _resolver.Setup(r => r.Resolve(SourceType.Bitstamp)).Returns(service.Object);
        var act = () => _provider.GetCandlesAsync(InstrumentType.BtcUsd, Start, End, [SourceType.Bitstamp]);

        // Act & Assert
        await act.Should().ThrowAsync<HttpRequestException>();
        VerifyNothingAdded();
        _unitOfWork.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupStored(SourceType source, Candle? candle)
    {
        _repository
            .Setup(r => r.GetAsync(Start, End, source, InstrumentType.BtcUsd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candle);
    }

    private void SetupRemote(SourceType source, CandleResponse? response)
    {
        var service = new Mock<IRequestService>();
        service
            .Setup(s => s.GetPriceAsync(InstrumentType.BtcUsd, Start, End, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        _resolver.Setup(r => r.Resolve(source)).Returns(service.Object);
    }

    private void VerifyNothingAdded()
    {
        _repository.Verify(r => r.AddAsync(
            It.IsAny<long>(), It.IsAny<long>(), It.IsAny<SourceType>(), It.IsAny<InstrumentType>(),
            It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()),
            Times.Never);
    }

    private static Candle Stored(decimal close)
    {
        return new()
        {
            PriceOpen = close - 1,
            PriceClose = close,
            PriceHigh = close + 1,
            PriceLow = close - 2,
            Volume = 10
        };
    }

    private static CandleResponse Remote(decimal close)
    {
        return new()
        {
            PriceOpen = close - 1,
            PriceClose = close,
            PriceHigh = close + 1,
            PriceLow = close - 2,
            Volume = 20
        };
    }
}
