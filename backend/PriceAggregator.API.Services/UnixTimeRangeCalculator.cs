using PriceAggregator.API.Services.Abstractions;

namespace PriceAggregator.API.Services;

public class UnixTimeRangeCalculator : IUnixTimeRangeCalculator
{
    private const long HourInMilliseconds = 3_600_000;

    private static readonly DateTime Origin = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    public (long Start, long End) GetRange(DateTime date)
    {
        var start = GetUnixTimestampMs(TruncateToHour(ToUtc(date)));

        return (start, start + HourInMilliseconds - 1);
    }

    private static DateTime ToUtc(DateTime date)
    {
        return date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : date.ToUniversalTime();
    }

    private static DateTime TruncateToHour(DateTime utc)
    {
        return utc.Date.AddHours(utc.Hour);
    }

    private static long GetUnixTimestampMs(DateTime dateUtc)
    {
        return (long)(dateUtc - Origin).TotalMilliseconds;
    }
}
