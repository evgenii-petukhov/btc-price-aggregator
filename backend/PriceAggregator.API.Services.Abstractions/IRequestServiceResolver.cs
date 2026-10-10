using PriceAggregator.Persistence.Enums;

namespace PriceAggregator.API.Services.Abstractions;

public interface IRequestServiceResolver
{
    IRequestService? Resolve(SourceType sourceType);
}
