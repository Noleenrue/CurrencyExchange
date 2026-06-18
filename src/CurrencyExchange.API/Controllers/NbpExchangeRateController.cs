using CurrencyExchange.Core.Exceptions;
using CurrencyExchange.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.API.Controllers;

/// <summary>
/// Exposes the NBP exchange rate API to consumers.
/// All endpoints are public (no authentication required).
/// </summary>
[ApiController]
[Route("api/nbp")]
[Produces("application/json")]
public class NbpExchangeRateController : ControllerBase
{
    private readonly INbpExchangeRateService _nbp;
    private readonly ILogger<NbpExchangeRateController> _logger;

    public NbpExchangeRateController(
        INbpExchangeRateService nbp,
        ILogger<NbpExchangeRateController> logger)
    {
        _nbp    = nbp;
        _logger = logger;
    }

    /// <summary>Gets today's exchange rate for a single currency.</summary>
    /// <param name="code">ISO 4217 currency code, e.g. USD, EUR.</param>
    /// <response code="200">Rate returned successfully.</response>
    /// <response code="404">No rate published today for this currency (weekend/holiday).</response>
    [HttpGet("rates/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentRate(string code)
    {
        try
        {
            var rate = await _nbp.GetCurrentRateAsync(code);
            return rate is null
                ? NotFound(new { message = $"No rate available today for '{code.ToUpperInvariant()}'." })
                : Ok(rate);
        }
        catch (NbpApiException ex) when (ex.StatusCode == 0)
        {
            _logger.LogError(ex, "NBP network error for {Code}", code);
            return StatusCode(503, new { message = "NBP API is currently unavailable. Please try again later." });
        }
        catch (NbpApiException ex)
        {
            return StatusCode(502, new { message = ex.Message });
        }
    }

    /// <summary>Gets today's mid rates for all currencies from NBP Table A.</summary>
    [HttpGet("rates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCurrentRates()
    {
        try
        {
            var response = await _nbp.GetAllCurrentRatesAsync();
            return Ok(response);
        }
        catch (NbpApiException ex) when (ex.StatusCode == 0)
        {
            _logger.LogError(ex, "NBP network error fetching all rates");
            return StatusCode(503, new { message = "NBP API is currently unavailable." });
        }
        catch (NbpApiException ex)
        {
            return StatusCode(502, new { message = ex.Message });
        }
    }

    /// <summary>Gets the exchange rate for a specific date.</summary>
    /// <param name="code">ISO 4217 currency code.</param>
    /// <param name="date">Date in yyyy-MM-dd format.</param>
    [HttpGet("rates/{code}/{date}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRateByDate(string code, string date)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd." });

        if (parsedDate > DateOnly.FromDateTime(DateTime.Today))
            return BadRequest(new { message = "Cannot query future exchange rates." });

        try
        {
            var rate = await _nbp.GetRateByDateAsync(code, parsedDate);
            return rate is null
                ? NotFound(new { message = $"No rate published for '{code.ToUpperInvariant()}' on {date}." })
                : Ok(rate);
        }
        catch (NbpApiException ex) when (ex.StatusCode == 0)
        {
            return StatusCode(503, new { message = "NBP API is currently unavailable." });
        }
        catch (NbpApiException ex)
        {
            return StatusCode(502, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets historical exchange rates for a currency within a date range.
    /// Maximum 93-day window per request; use the 'unlimited' endpoint for longer ranges.
    /// </summary>
    /// <param name="code">ISO 4217 currency code.</param>
    /// <param name="from">Start date (yyyy-MM-dd).</param>
    /// <param name="to">End date (yyyy-MM-dd).</param>
    [HttpGet("rates/{code}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistoricalRates(
        string code,
        [FromQuery] string from,
        [FromQuery] string to)
    {
        if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
            return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd." });

        if (fromDate > toDate)
            return BadRequest(new { message = "'from' must not be after 'to'." });

        try
        {
            var response = await _nbp.GetHistoricalRatesAsync(code, fromDate, toDate);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NbpApiException ex) when (ex.StatusCode == 0)
        {
            return StatusCode(503, new { message = "NBP API is currently unavailable." });
        }
        catch (NbpApiException ex)
        {
            return StatusCode(502, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets historical exchange rates for any date range — automatically
    /// splits requests longer than 93 days into multiple NBP calls.
    /// </summary>
    [HttpGet("rates/{code}/history/full")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistoricalRatesFull(
        string code,
        [FromQuery] string from,
        [FromQuery] string to)
    {
        if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
            return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd." });

        if (fromDate > toDate)
            return BadRequest(new { message = "'from' must not be after 'to'." });

        try
        {
            var response = await _nbp.GetHistoricalRatesUnlimitedAsync(code, fromDate, toDate);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (NbpApiException ex) when (ex.StatusCode == 0)
        {
            return StatusCode(503, new { message = "NBP API is currently unavailable." });
        }
        catch (NbpApiException ex)
        {
            return StatusCode(502, new { message = ex.Message });
        }
    }
}
