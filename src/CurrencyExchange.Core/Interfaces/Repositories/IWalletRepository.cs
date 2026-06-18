using CurrencyExchange.Core.Entities;

namespace CurrencyExchange.Core.Interfaces.Repositories;

public interface IWalletRepository : IRepository<Wallet>
{
    Task<IEnumerable<Wallet>> GetUserWalletsAsync(int userId);
    Task<Wallet?> GetUserWalletAsync(int userId, int currencyId);
    Task<Wallet> GetOrCreateWalletAsync(int userId, int currencyId);
}
