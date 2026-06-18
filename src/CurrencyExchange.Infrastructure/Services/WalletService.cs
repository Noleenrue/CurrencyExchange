using CurrencyExchange.Core.DTOs.Wallet;
using CurrencyExchange.Core.Interfaces.Repositories;
using CurrencyExchange.Core.Interfaces.Services;

namespace CurrencyExchange.Infrastructure.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _repo;

    public WalletService(IWalletRepository repo) => _repo = repo;

    public async Task<WalletSummaryDto> GetWalletSummaryAsync(int userId)
    {
        var wallets = (await _repo.GetUserWalletsAsync(userId)).ToList();

        // PLN wallet is CurrencyId = 1 (seeded as PLN)
        const int plnCurrencyId = 1;
        var pln     = wallets.FirstOrDefault(w => w.CurrencyId == plnCurrencyId);
        var foreign = wallets
            .Where(w => w.CurrencyId != plnCurrencyId)
            .Select(w => new WalletDto(
                w.Id,
                w.CurrencyId,
                w.Currency?.Code ?? "",
                w.Currency?.Name ?? "",
                w.Balance));

        return new WalletSummaryDto(pln?.Balance ?? 0, foreign);
    }

    public async Task<WalletDto?> GetWalletAsync(int userId, int currencyId)
    {
        var w = await _repo.GetUserWalletAsync(userId, currencyId);
        if (w is null) return null;
        return new WalletDto(w.Id, w.CurrencyId, w.Currency?.Code ?? "", w.Currency?.Name ?? "", w.Balance);
    }
}
