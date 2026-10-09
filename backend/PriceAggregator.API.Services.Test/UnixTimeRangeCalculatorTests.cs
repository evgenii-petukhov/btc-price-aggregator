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
    public void GetRange_MidDayUtc_ReturnsPreviousDayStartAndGivenDayEnd()
    {
        var date = new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc);

        var (start, end) = _service.GetRange(date);

        start.Should().Be(ToMs(new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc)));
        end.Should().Be(ToMs(new DateTime(2026, 9, 8, 23, 59, 59, 999, DateTimeKind.Utc)));
    }

    [Test]
    public void GetRange_KnownDate_MatchesExpectedConstants()
    {
        var date = new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc);

        var (start, end) = _service.GetRange(date);

        start.Should().Be(1788739200000);
        end.Should().Be(1788911999999);
    }

    [TestCase(0, 0, 0)]
    [TestCase(23, 59, 59)]
    public void GetRange_AnyTimeOfSameDay_ReturnsSameRange(int hour, int minute, int second)
    {
        var reference = _service.GetRange(new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc));

        var result = _service.GetRange(new DateTime(2026, 9, 8, hour, minute, second, DateTimeKind.Utc));

        result.Should().Be(reference);
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
        var local = new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Local);
        var expected = _service.GetRange(local.ToUniversalTime());

        _service.GetRange(local).Should().Be(expected);
    }

    [Test]
    public void GetRange_FirstDayOfYear_StartsOnLastDayOfPreviousYear()
    {
        var date = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var (start, end) = _service.GetRange(date);

        start.Should().Be(ToMs(new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc)));
        end.Should().Be(ToMs(new DateTime(2026, 1, 1, 23, 59, 59, 999, DateTimeKind.Utc)));
    }

    [Test]
    public void GetDayStartInSeconds_MidDayUtc_ReturnsSameDayStart()
    {
        var date = new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc);

        var result = _service.GetDayStartInSeconds(date);

        result.Should().Be(new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds());
    }

    [TestCase(0, 0, 0)]
    [TestCase(23, 59, 59)]
    public void GetDayStartInSeconds_AnyTimeOfSameDay_ReturnsSameValue(int hour, int minute, int second)
    {
        var reference = _service.GetDayStartInSeconds(new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc));

        var result = _service.GetDayStartInSeconds(new DateTime(2026, 9, 8, hour, minute, second, DateTimeKind.Utc));

        result.Should().Be(reference);
    }

    [Test]
    public void GetDayStartInSeconds_UnspecifiedKind_IsTreatedAsUtc()
    {
        var unspecified = new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Unspecified);
        var utc = new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc);

        _service.GetDayStartInSeconds(unspecified).Should().Be(_service.GetDayStartInSeconds(utc));
    }

    [Test]
    public void GetDayStartInSeconds_LocalKind_IsConvertedToUtcBeforeCalculation()
    {
        var local = new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Local);
        var expected = _service.GetDayStartInSeconds(local.ToUniversalTime());

        _service.GetDayStartInSeconds(local).Should().Be(expected);
    }

    [Test]
    public void GetDayStartInSeconds_FirstDayOfYear_ReturnsJanuaryFirstStart()
    {
        var date = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var result = _service.GetDayStartInSeconds(date);

        result.Should().Be(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds());
    }

    private static long ToMs(DateTime utc)
    {
        return new DateTimeOffset(utc).ToUnixTimeMilliseconds();
    }
}
