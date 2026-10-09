using Microsoft.AspNetCore.Mvc;
using PriceAggregator.API.Models;

namespace PriceAggregator.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PriceController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PriceDto>> GetAsync(CancellationToken cancellationToken)
    {
        return Ok();
    }
}
