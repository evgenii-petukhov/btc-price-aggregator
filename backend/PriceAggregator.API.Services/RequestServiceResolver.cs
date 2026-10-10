using PriceAggregator.API.Services.Abstractions;
using PriceAggregator.Persistence.Enums;

namespace PriceAggregator.API.Services;

public class RequestServiceResolver(
    IEnumerable<IRequestService> requestServices) : IRequestServiceResolver
{
    private readonly Lazy<Dictionary<SourceType, IRequestService>> _requestServices = 
        new(() => requestServices.ToDictionary(x => x.SourceType));

    public IRequestService? Resolve(SourceType sourceType)
    {
        return _requestServices.Value.GetValueOrDefault(sourceType);
    }
}
