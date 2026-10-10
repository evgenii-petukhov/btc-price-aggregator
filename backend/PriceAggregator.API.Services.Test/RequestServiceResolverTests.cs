using FluentAssertions;
using Moq;
using NUnit.Framework;
using PriceAggregator.API.Services.Abstractions;
using PriceAggregator.Persistence.Enums;

namespace PriceAggregator.API.Services.Test;

[TestFixture]
public class RequestServiceResolverTests
{
    [Test]
    public void Resolve_RegisteredSource_ReturnsMatchingService()
    {
        // Arrange
        var bitstamp = CreateService(SourceType.Bitstamp).Object;
        var bitfinex = CreateService(SourceType.Bitfinex).Object;
        var resolver = new RequestServiceResolver([bitstamp, bitfinex]);

        // Act & Assert
        resolver.Resolve(SourceType.Bitfinex).Should().BeSameAs(bitfinex);
        resolver.Resolve(SourceType.Bitstamp).Should().BeSameAs(bitstamp);
    }

    [Test]
    public void Resolve_UnregisteredSource_ReturnsNull()
    {
        // Arrange
        var resolver = new RequestServiceResolver([CreateService(SourceType.Bitstamp).Object]);

        // Act & Assert
        resolver.Resolve(SourceType.Bitfinex).Should().BeNull();
    }

    [Test]
    public void Resolve_NoServices_ReturnsNull()
    {
        // Arrange
        var resolver = new RequestServiceResolver([]);

        // Act & Assert
        resolver.Resolve(SourceType.Bitstamp).Should().BeNull();
    }

    [Test]
    public void Resolve_DuplicateSourceTypes_Throws()
    {
        // Arrange
        var resolver = new RequestServiceResolver(
            [CreateService(SourceType.Bitstamp).Object, CreateService(SourceType.Bitstamp).Object]);
        var act = () => resolver.Resolve(SourceType.Bitstamp);

        // Act & Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_DoesNotEnumerateServicesUntilResolve()
    {
        // Arrange
        var enumerated = false;
        IEnumerable<IRequestService> Services()
        {
            enumerated = true;
            yield return CreateService(SourceType.Bitstamp).Object;
        }

        // Act
        var resolver = new RequestServiceResolver(Services());

        // Assert
        enumerated.Should().BeFalse();
        resolver.Resolve(SourceType.Bitstamp);
        enumerated.Should().BeTrue();
    }

    private static Mock<IRequestService> CreateService(SourceType sourceType)
    {
        var mock = new Mock<IRequestService>();
        mock.SetupGet(s => s.SourceType).Returns(sourceType);
        return mock;
    }
}
