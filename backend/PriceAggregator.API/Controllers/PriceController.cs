using Microsoft.AspNetCore.Mvc;
using PriceAggregator.API.Models.Dtos;
using PriceAggregator.API.Services.Abstractions;
using PriceAggregator.Persistence.Enums;
using System.ComponentModel.DataAnnotations;

namespace PriceAggregator.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PriceController(
    IPriceSummaryService priceSummaryService) : ControllerBase
{
    private readonly IPriceSummaryService _priceSummaryService = priceSummaryService;

    [HttpGet]
    public async Task<ActionResult<PriceSummaryDto>> GetAsync(
        DateTime date,
        [Required, EnumDataType(typeof(InstrumentType))] InstrumentType? instrument,
        CancellationToken cancellationToken)
    {
        var result = await _priceSummaryService.GetSummaryAsync(date, instrument!.Value, cancellationToken);
        return Ok(result);
    }
}
