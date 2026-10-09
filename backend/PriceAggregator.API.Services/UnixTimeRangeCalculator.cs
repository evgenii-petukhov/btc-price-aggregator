using PriceAggregator.API.Services.Abstractions;

namespace PriceAggregator.API.Services;

public class UnixTimeRangeCalculator : IUnixTimeRangeCalculator
{
    private static readonly DateTime Origin = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    public (long Start, long End) GetRange(DateTime date)
    {
        var utc = ToUtc(date);

        var start = utc.Date.AddDays(-1);
        var end = utc.Date.AddDays(1).AddMilliseconds(-1);

        return (GetUnixTimestampMs(start), GetUnixTimestampMs(end));
    }

    public long GetDayStartInSeconds(DateTime date)
    {
        return (long)(ToUtc(date).Date - Origin).TotalSeconds;
    }

    private static DateTime ToUtc(DateTime date)
    {
        return date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : date.ToUniversalTime();
    }

    private static long GetUnixTimestampMs(DateTime dateUtc)
    {
        return (long)(dateUtc - Origin).TotalMilliseconds;
    }
}
