using Microsoft.AspNetCore.Mvc;
using PriceAggregator.API.Models.Dtos;
using PriceAggregator.API.Services.Abstractions;

namespace PriceAggregator.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PriceController(
    IPriceSummaryService priceSummaryService) : ControllerBase
{
    private readonly IPriceSummaryService _priceSummaryService = priceSummaryService;

    [HttpGet]
    public async Task<ActionResult<PriceSummaryDto>> GetAsync(DateTime date, CancellationToken cancellationToken)
    {
        var result = await _priceSummaryService.GetSummaryAsync(date, cancellationToken);
        return Ok(result);
    }
}
