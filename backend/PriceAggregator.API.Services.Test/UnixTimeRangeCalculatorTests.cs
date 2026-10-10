using FluentAssertions;
using NUnit.Framework;

namespace PriceAggregator.API.Services.Test;

[TestFixture]
public class UnixTimeRangeCalculatorTests
{
    private UnixTimeRangeCalculator _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new UnixTimeRangeCalculator();
    }

    [Test]
    public void GetRange_HourStart_ReturnsStartAndInclusiveEndOfThatHour()
    {
        // Arrange
        var date = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var (start, end) = _service.GetRange(date);

        // Assert
        start.Should().Be(1672531200000);
        end.Should().Be(1672534799999);
    }

    [Test]
    public void GetRange_MinutesAndSeconds_AreTruncatedToHour()
    {
        // Arrange
        var date = new DateTime(2026, 9, 8, 10, 45, 30, DateTimeKind.Utc);

        // Act
        var (start, end) = _service.GetRange(date);

        // Assert
        start.Should().Be(ToMs(new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc)));
        end.Should().Be(ToMs(new DateTime(2026, 9, 8, 10, 59, 59, 999, DateTimeKind.Utc)));
    }

    [TestCase(0, 0)]
    [TestCase(59, 59)]
    public void GetRange_AnyTimeWithinSameHour_ReturnsSameRange(int minute, int second)
    {
        // Arrange
        var reference = _service.GetRange(new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc));

        // Act
        var result = _service.GetRange(new DateTime(2026, 9, 8, 10, minute, second, DateTimeKind.Utc));

        // Assert
        result.Should().Be(reference);
    }

    [Test]
    public void GetRange_NextHour_ShiftsByOneHour()
    {
        // Arrange
        var (Start, End) = _service.GetRange(new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc));

        // Act
        var second = _service.GetRange(new DateTime(2026, 9, 8, 11, 0, 0, DateTimeKind.Utc));

        // Assert
        (second.Start - Start).Should().Be(3_600_000);
        (second.End - End).Should().Be(3_600_000);
    }

    [Test]
    public void GetRange_UnspecifiedKind_IsTreatedAsUtc()
    {
        // Arrange
        var unspecified = new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Unspecified);
        var utc = new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        _service.GetRange(unspecified).Should().Be(_service.GetRange(utc));
    }

    [Test]
    public void GetRange_LocalKind_IsConvertedToUtcBeforeCalculation()
    {
        // Arrange
        var local = new DateTime(2026, 9, 8, 10, 15, 0, DateTimeKind.Local);

        // Act
        var expected = _service.GetRange(local.ToUniversalTime());

        // Assert
        _service.GetRange(local).Should().Be(expected);
    }

    [Test]
    public void GetRange_LastHourOfYear_EndsOnYearBoundary()
    {
        // Arrange
        var date = new DateTime(2025, 12, 31, 23, 30, 0, DateTimeKind.Utc);

        // Act
        var (start, end) = _service.GetRange(date);

        // Assert
        start.Should().Be(ToMs(new DateTime(2025, 12, 31, 23, 0, 0, DateTimeKind.Utc)));
        end.Should().Be(ToMs(new DateTime(2025, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc)));
    }

    private static long ToMs(DateTime utc)
    {
        return new DateTimeOffset(utc).ToUnixTimeMilliseconds();
    }
}
