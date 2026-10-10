namespace PriceAggregator.API.Services.Abstractions;

public interface IUnixTimeRangeCalculator
{
    (long Start, long End) GetRange(DateTime date);
}
