using CurrencyExchange.Core.DTOs.Exchange;
using CurrencyExchange.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.API.Controllers;

/// <summary>Endpoints for buying and selling foreign currency.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class CurrencyExchangeController : ControllerBase
{
    private readonly ICurrencyExchangeService _exchangeService;

    public CurrencyExchangeController(ICurrencyExchangeService exchangeService)
        => _exchangeService = exchangeService;

    // ── POST /api/currencyexchange/buy ────────────────────────────────────────

    /// <summary>
    /// Buys foreign currency: deducts PLN at the NBP ask rate and credits the
    /// foreign wallet. Returns the transaction result with updated balances.
    /// </summary>
    /// <response code="200">Exchange completed successfully.</response>
    /// <response code="400">Validation error or insufficient balance.</response>
    /// <response code="404">User wallet not found.</response>
    /// <response code="503">NBP rate unavailable (markets closed / holiday).</response>
    [HttpPost("buy")]
    [ProducesResponseType(typeof(ExchangeResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Buy([FromBody] BuyCurrencyRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        return await ExecuteExchangeAsync(
            () => _exchangeService.BuyCurrencyAsync(request, ct));
    }

    // ── POST /api/currencyexchange/sell ───────────────────────────────────────

    /// <summary>
    /// Sells foreign currency: deducts the foreign amount at the NBP bid rate and
    /// credits the PLN wallet. Returns the transaction result with updated balances.
    /// </summary>
    /// <response code="200">Exchange completed successfully.</response>
    /// <response code="400">Validation error or insufficient balance.</response>
    /// <response code="404">User wallet not found.</response>
    /// <response code="503">NBP rate unavailable (markets closed / holiday).</response>
    [HttpPost("sell")]
    [ProducesResponseType(typeof(ExchangeResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Sell([FromBody] SellCurrencyRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        return await ExecuteExchangeAsync(
            () => _exchangeService.SellCurrencyAsync(request, ct));
    }

    // ─── Helper ───────────────────────────────────────────────────────────────

    /// <summary>Shared exception-to-HTTP-status mapping for exchange operations.</summary>
    private async Task<IActionResult> ExecuteExchangeAsync(Func<Task<ExchangeResult>> operation)
    {
        try
        {
            var result = await operation();
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No NBP rate available"))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Covers: unsupported currency, insufficient balance
            return BadRequest(new { message = ex.Message });
        }
    }
}
