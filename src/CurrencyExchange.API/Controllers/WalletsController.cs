using CurrencyExchange.Core.DTOs.Wallet;
using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.API.Controllers;

/// <summary>CRUD operations for Wallets.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class WalletsController : ControllerBase
{
    private readonly IWalletRepository   _walletRepo;
    private readonly IUserRepository     _userRepo;
    private readonly ICurrencyRepository _currencyRepo;

    public WalletsController(
        IWalletRepository   walletRepo,
        IUserRepository     userRepo,
        ICurrencyRepository currencyRepo)
    {
        _walletRepo   = walletRepo;
        _userRepo     = userRepo;
        _currencyRepo = currencyRepo;
    }

    // ── GET /api/wallets ─────────────────────────────────────────────────────

    /// <summary>Returns all wallets.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WalletDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var wallets = await _walletRepo.GetAllAsync();
        return Ok(wallets.Select(ToDto));
    }

    // ── GET /api/wallets/{id} ────────────────────────────────────────────────

    /// <summary>Returns a single wallet by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var wallet = await _walletRepo.GetByIdAsync(id);
        return wallet is null
            ? NotFound(new { message = $"Wallet {id} not found." })
            : Ok(ToDto(wallet));
    }

    // ── GET /api/wallets/user/{userId} ───────────────────────────────────────

    /// <summary>Returns all wallets belonging to a user.</summary>
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(typeof(IEnumerable<WalletDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUser(int userId)
    {
        if (!await _userRepo.ExistsAsync(u => u.Id == userId))
            return NotFound(new { message = $"User {userId} not found." });

        var wallets = await _walletRepo.GetUserWalletsAsync(userId);
        return Ok(wallets.Select(ToDto));
    }

    // ── POST /api/wallets ────────────────────────────────────────────────────

    /// <summary>Creates a new wallet for a user/currency pair.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateWalletDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!await _userRepo.ExistsAsync(u => u.Id == dto.UserId))
            return NotFound(new { message = $"User {dto.UserId} not found." });

        if (!await _currencyRepo.ExistsAsync(c => c.Id == dto.CurrencyId))
            return NotFound(new { message = $"Currency {dto.CurrencyId} not found." });

        if (await _walletRepo.ExistsAsync(w => w.UserId == dto.UserId && w.CurrencyId == dto.CurrencyId))
            return Conflict(new
            {
                message = $"Wallet already exists for user {dto.UserId} and currency {dto.CurrencyId}."
            });

        var wallet = new Wallet
        {
            UserId     = dto.UserId,
            CurrencyId = dto.CurrencyId,
            Balance    = dto.InitialBalance
        };

        await _walletRepo.AddAsync(wallet);

        // Reload with navigation properties for the response
        var created = await _walletRepo.GetByIdAsync(wallet.Id);
        return CreatedAtAction(nameof(GetById), new { id = wallet.Id }, ToDto(created!));
    }

    // ── PUT /api/wallets/{id} ────────────────────────────────────────────────

    /// <summary>Updates the balance of a wallet.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWalletDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var wallet = await _walletRepo.GetByIdAsync(id);
        if (wallet is null)
            return NotFound(new { message = $"Wallet {id} not found." });

        wallet.Balance = dto.Balance;
        await _walletRepo.UpdateAsync(wallet);
        return NoContent();
    }

    // ── DELETE /api/wallets/{id} ─────────────────────────────────────────────

    /// <summary>Deletes a wallet.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var wallet = await _walletRepo.GetByIdAsync(id);
        if (wallet is null)
            return NotFound(new { message = $"Wallet {id} not found." });

        await _walletRepo.DeleteAsync(wallet);
        return NoContent();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static WalletDto ToDto(Wallet w) => new(
        w.Id,
        w.CurrencyId,
        w.Currency?.Code ?? string.Empty,
        w.Currency?.Name ?? string.Empty,
        w.Balance
    );
}
