using PriceAggregator.API.Services.Abstractions;

namespace PriceAggregator.API.Services;

public class UnixTimeRangeCalculator : IUnixTimeRangeCalculator
{
    private static readonly DateTime Origin = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    public (long Start, long End) GetRange(DateTime date)
    {
        var utc = date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : date.ToUniversalTime();

        var start = utc.Date.AddDays(-1);
        var end = utc.Date.AddDays(1).AddMilliseconds(-1);

        return (GetUnixTimestampMs(start), GetUnixTimestampMs(end));
    }

    private static long GetUnixTimestampMs(DateTime dateUtc)
    {
        return (long)(dateUtc - Origin).TotalMilliseconds;
    }
}
