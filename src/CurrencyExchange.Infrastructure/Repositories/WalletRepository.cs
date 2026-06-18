using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Interfaces.Repositories;
using CurrencyExchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Infrastructure.Repositories;

public class WalletRepository : Repository<Wallet>, IWalletRepository
{
    public WalletRepository(CurrencyExchangeDbContext context) : base(context) { }

    public async Task<IEnumerable<Wallet>> GetUserWalletsAsync(int userId)
        => await _dbSet
            .Include(w => w.Currency)
            .Where(w => w.UserId == userId)
            .ToListAsync();

    public async Task<Wallet?> GetUserWalletAsync(int userId, int currencyId)
        => await _dbSet
            .Include(w => w.Currency)
            .FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyId == currencyId);

    public async Task<Wallet> GetOrCreateWalletAsync(int userId, int currencyId)
    {
        var wallet = await GetUserWalletAsync(userId, currencyId);
        if (wallet is not null) return wallet;

        wallet = new Wallet { UserId = userId, CurrencyId = currencyId, Balance = 0 };
        await AddAsync(wallet);
        return wallet;
    }
}
