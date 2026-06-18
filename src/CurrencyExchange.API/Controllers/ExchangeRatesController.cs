using CurrencyExchange.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExchangeRatesController : ControllerBase
{
    private readonly INbpService _nbpService;

    public ExchangeRatesController(INbpService nbpService) => _nbpService = nbpService;

    [HttpGet("current")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrent() => Ok(await _nbpService.GetCurrentRatesAsync());

    [HttpGet("current/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrentByCode(string code)
    {
        var rate = await _nbpService.GetCurrentRateAsync(code);
        return rate is null ? NotFound() : Ok(rate);
    }

    [HttpGet("historical/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHistorical(string code, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var rates = await _nbpService.GetHistoricalRatesAsync(code, from, to);
        return Ok(rates);
    }

    [HttpPost("sync")]
    [Authorize]
    public async Task<IActionResult> Sync()
    {
        await _nbpService.SyncRatesAsync();
        return Ok(new { message = "Rates synced successfully." });
    }
}
