using CurrencyExchange.Core.DTOs.Currency;
using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.API.Controllers;

/// <summary>CRUD operations for Currencies.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CurrenciesController : ControllerBase
{
    private readonly ICurrencyRepository _repo;

    public CurrenciesController(ICurrencyRepository repo) => _repo = repo;

    // ── GET /api/currencies ──────────────────────────────────────────────────

    /// <summary>Returns all currencies.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CurrencyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
        => Ok((await _repo.GetAllAsync()).Select(ToDto));

    // ── GET /api/currencies/active ───────────────────────────────────────────

    /// <summary>Returns only active currencies.</summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<CurrencyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive()
        => Ok((await _repo.GetActiveCurrenciesAsync()).Select(ToDto));

    // ── GET /api/currencies/{id} ─────────────────────────────────────────────

    /// <summary>Returns a currency by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var currency = await _repo.GetByIdAsync(id);
        return currency is null
            ? NotFound(new { message = $"Currency {id} not found." })
            : Ok(ToDto(currency));
    }

    // ── GET /api/currencies/code/{code} ─────────────────────────────────────

    /// <summary>Returns a currency by ISO code (e.g. USD, EUR).</summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var currency = await _repo.GetByCodeAsync(code);
        return currency is null
            ? NotFound(new { message = $"Currency '{code.ToUpperInvariant()}' not found." })
            : Ok(ToDto(currency));
    }

    // ── POST /api/currencies ─────────────────────────────────────────────────

    /// <summary>Creates a new currency.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCurrencyDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var code = dto.Code.Trim().ToUpperInvariant();

        if (await _repo.ExistsAsync(c => c.Code == code))
            return Conflict(new { message = $"Currency '{code}' already exists." });

        var currency = new Currency
        {
            Code     = code,
            Name     = dto.Name.Trim(),
            Symbol   = dto.Symbol.Trim(),
            IsActive = true
        };

        await _repo.AddAsync(currency);
        return CreatedAtAction(nameof(GetById), new { id = currency.Id }, ToDto(currency));
    }

    // ── PUT /api/currencies/{id} ─────────────────────────────────────────────

    /// <summary>Updates name, symbol, and active status of a currency.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCurrencyDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var currency = await _repo.GetByIdAsync(id);
        if (currency is null)
            return NotFound(new { message = $"Currency {id} not found." });

        currency.Name     = dto.Name.Trim();
        currency.Symbol   = dto.Symbol.Trim();
        currency.IsActive = dto.IsActive;

        await _repo.UpdateAsync(currency);
        return NoContent();
    }

    // ── DELETE /api/currencies/{id} ──────────────────────────────────────────

    /// <summary>Deletes a currency. Returns 409 if referenced by wallets or transactions.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var currency = await _repo.GetByIdAsync(id);
        if (currency is null)
            return NotFound(new { message = $"Currency {id} not found." });

        try
        {
            await _repo.DeleteAsync(currency);
            return NoContent();
        }
        catch (Exception ex) when (ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
        {
            return Conflict(new
            {
                message = "Cannot delete: currency is referenced by wallets or transactions."
            });
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static CurrencyDto ToDto(Currency c)
        => new(c.Id, c.Code, c.Name, c.Symbol, c.IsActive);
}

