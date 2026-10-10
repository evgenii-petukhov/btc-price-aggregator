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
        var date = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var (start, end) = _service.GetRange(date);

        start.Should().Be(1672531200000);
        end.Should().Be(1672534799999);
    }

    [Test]
    public void GetRange_MinutesAndSeconds_AreTruncatedToHour()
    {
        var date = new DateTime(2026, 9, 8, 10, 45, 30, DateTimeKind.Utc);

        var (start, end) = _service.GetRange(date);

        start.Should().Be(ToMs(new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc)));
        end.Should().Be(ToMs(new DateTime(2026, 9, 8, 10, 59, 59, 999, DateTimeKind.Utc)));
    }

    [TestCase(0, 0)]
    [TestCase(59, 59)]
    public void GetRange_AnyTimeWithinSameHour_ReturnsSameRange(int minute, int second)
    {
        var reference = _service.GetRange(new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc));

        var result = _service.GetRange(new DateTime(2026, 9, 8, 10, minute, second, DateTimeKind.Utc));

        result.Should().Be(reference);
    }

    [Test]
    public void GetRange_NextHour_ShiftsByOneHour()
    {
        var first = _service.GetRange(new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc));
        var second = _service.GetRange(new DateTime(2026, 9, 8, 11, 0, 0, DateTimeKind.Utc));

        (second.Start - first.Start).Should().Be(3_600_000);
        (second.End - first.End).Should().Be(3_600_000);
    }

    [Test]
    public void GetRange_UnspecifiedKind_IsTreatedAsUtc()
    {
        var unspecified = new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Unspecified);
        var utc = new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc);

        _service.GetRange(unspecified).Should().Be(_service.GetRange(utc));
    }

    [Test]
    public void GetRange_LocalKind_IsConvertedToUtcBeforeCalculation()
    {
        var local = new DateTime(2026, 9, 8, 10, 15, 0, DateTimeKind.Local);
        var expected = _service.GetRange(local.ToUniversalTime());

        _service.GetRange(local).Should().Be(expected);
    }

    [Test]
    public void GetRange_LastHourOfYear_EndsOnYearBoundary()
    {
        var date = new DateTime(2025, 12, 31, 23, 30, 0, DateTimeKind.Utc);

        var (start, end) = _service.GetRange(date);

        start.Should().Be(ToMs(new DateTime(2025, 12, 31, 23, 0, 0, DateTimeKind.Utc)));
        end.Should().Be(ToMs(new DateTime(2025, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc)));
    }

    private static long ToMs(DateTime utc)
    {
        return new DateTimeOffset(utc).ToUnixTimeMilliseconds();
    }
}
