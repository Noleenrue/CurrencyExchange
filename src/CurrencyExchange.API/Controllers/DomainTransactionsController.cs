using CurrencyExchange.Core.DTOs.DomainTransaction;
using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Enums;
using CurrencyExchange.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.API.Controllers;

/// <summary>CRUD operations for domain Transactions (buy/sell history).</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class DomainTransactionsController : ControllerBase
{
    private readonly IDomainTransactionRepository _repo;
    private readonly IUserRepository              _userRepo;
    private readonly ICurrencyRepository          _currencyRepo;

    public DomainTransactionsController(
        IDomainTransactionRepository repo,
        IUserRepository              userRepo,
        ICurrencyRepository          currencyRepo)
    {
        _repo         = repo;
        _userRepo     = userRepo;
        _currencyRepo = currencyRepo;
    }

    // ── GET /api/domaintransactions ──────────────────────────────────────────

    /// <summary>Returns all transactions with User and Currency details.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DomainTransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var list = await _repo.GetAllAsync();
        return Ok(list.Select(ToDto));
    }

    // ── GET /api/domaintransactions/{id} ─────────────────────────────────────

    /// <summary>Returns a single transaction (with details) by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DomainTransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var tx = await _repo.GetWithDetailsAsync(id);
        return tx is null
            ? NotFound(new { message = $"Transaction {id} not found." })
            : Ok(ToDto(tx));
    }

    // ── GET /api/domaintransactions/user/{userId} ────────────────────────────

    /// <summary>Returns all transactions for a specific user.</summary>
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(typeof(IEnumerable<DomainTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUser(int userId)
    {
        if (!await _userRepo.ExistsAsync(u => u.Id == userId))
            return NotFound(new { message = $"User {userId} not found." });

        return Ok((await _repo.GetByUserIdAsync(userId)).Select(ToDto));
    }

    // ── GET /api/domaintransactions/currency/{currencyId} ───────────────────

    /// <summary>Returns all transactions for a specific currency.</summary>
    [HttpGet("currency/{currencyId:int}")]
    [ProducesResponseType(typeof(IEnumerable<DomainTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCurrency(int currencyId)
    {
        if (!await _currencyRepo.ExistsAsync(c => c.Id == currencyId))
            return NotFound(new { message = $"Currency {currencyId} not found." });

        return Ok((await _repo.GetByCurrencyIdAsync(currencyId)).Select(ToDto));
    }

    // ── GET /api/domaintransactions/type/{type} ──────────────────────────────

    /// <summary>Returns transactions filtered by type (Buy or Sell).</summary>
    [HttpGet("type/{type}")]
    [ProducesResponseType(typeof(IEnumerable<DomainTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByType(string type)
    {
        if (!Enum.TryParse<TransactionType>(type, ignoreCase: true, out var txType))
            return BadRequest(new { message = $"Invalid transaction type '{type}'. Use 'Buy' or 'Sell'." });

        return Ok((await _repo.GetByTypeAsync(txType)).Select(ToDto));
    }

    // ── GET /api/domaintransactions/range?from=&to= ──────────────────────────

    /// <summary>Returns transactions within a date range (UTC).</summary>
    [HttpGet("range")]
    [ProducesResponseType(typeof(IEnumerable<DomainTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        if (from > to)
            return BadRequest(new { message = "'from' must be earlier than or equal to 'to'." });

        return Ok((await _repo.GetByDateRangeAsync(from, to)).Select(ToDto));
    }

    // ── POST /api/domaintransactions ─────────────────────────────────────────

    /// <summary>Records a new transaction.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(DomainTransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateDomainTransactionDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!await _userRepo.ExistsAsync(u => u.Id == dto.UserId))
            return NotFound(new { message = $"User {dto.UserId} not found." });

        if (!await _currencyRepo.ExistsAsync(c => c.Id == dto.CurrencyId))
            return NotFound(new { message = $"Currency {dto.CurrencyId} not found." });

        var tx = new Transaction
        {
            UserId          = dto.UserId,
            CurrencyId      = dto.CurrencyId,
            Amount          = dto.Amount,
            ExchangeRate    = dto.ExchangeRate,
            TransactionType = dto.TransactionType,
            TransactionDate = DateTime.UtcNow
        };

        await _repo.AddAsync(tx);

        // Reload with navigation properties for the response
        var created = await _repo.GetWithDetailsAsync(tx.Id);
        return CreatedAtAction(nameof(GetById), new { id = tx.Id }, ToDto(created!));
    }

    // ── PUT /api/domaintransactions/{id} ─────────────────────────────────────

    /// <summary>Updates amount, exchange rate, and type of an existing transaction.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDomainTransactionDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var tx = await _repo.GetByIdAsync(id);
        if (tx is null)
            return NotFound(new { message = $"Transaction {id} not found." });

        tx.Amount          = dto.Amount;
        tx.ExchangeRate    = dto.ExchangeRate;
        tx.TransactionType = dto.TransactionType;

        await _repo.UpdateAsync(tx);
        return NoContent();
    }

    // ── DELETE /api/domaintransactions/{id} ──────────────────────────────────

    /// <summary>Deletes a transaction record.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var tx = await _repo.GetByIdAsync(id);
        if (tx is null)
            return NotFound(new { message = $"Transaction {id} not found." });

        await _repo.DeleteAsync(tx);
        return NoContent();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static DomainTransactionDto ToDto(Transaction t) => new(
        t.Id,
        t.UserId,
        t.CurrencyId,
        t.Currency?.Code     ?? string.Empty,
        t.User?.FullName     ?? string.Empty,
        t.Amount,
        t.ExchangeRate,
        t.TransactionType,
        t.TransactionDate
    );
}
