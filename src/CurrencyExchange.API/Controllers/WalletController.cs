using CurrencyExchange.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CurrencyExchange.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _service;
    private readonly CurrencyExchange.Core.Interfaces.Repositories.IUserRepository _userRepo;

    public WalletController(
        IWalletService service,
        CurrencyExchange.Core.Interfaces.Repositories.IUserRepository userRepo)
    {
        _service  = service;
        _userRepo = userRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetSummary()
    {
        var identityId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var domainUser = await _userRepo.GetByIdentityIdAsync(identityId);
        if (domainUser is null) return NotFound(new { message = "Domain user not found." });
        return Ok(await _service.GetWalletSummaryAsync(domainUser.Id));
    }

    [HttpGet("{currencyId:int}")]
    public async Task<IActionResult> GetWallet(int currencyId)
    {
        var identityId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var domainUser = await _userRepo.GetByIdentityIdAsync(identityId);
        if (domainUser is null) return NotFound(new { message = "Domain user not found." });
        var wallet = await _service.GetWalletAsync(domainUser.Id, currencyId);
        return wallet is null ? NotFound() : Ok(wallet);
    }
}
