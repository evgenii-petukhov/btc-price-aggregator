using Microsoft.AspNetCore.Mvc;
using PriceAggregator.API.Models.Dtos;
using PriceAggregator.API.Services.Abstractions;

namespace PriceAggregator.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PriceController(
    IPriceService priceService) : ControllerBase
{
    private readonly IPriceService _priceService = priceService;

    [HttpGet]
    public async Task<ActionResult<PriceSummaryDto>> GetAsync(DateTime date, CancellationToken cancellationToken)
    {
        var result = await _priceService.GetPriceAsync(date, cancellationToken);
        return Ok(result);
    }
}
