using CurrencyExchange.Core.DTOs.Wallet;

namespace CurrencyExchange.Core.Interfaces.Services;

public interface IWalletService
{
    Task<WalletSummaryDto> GetWalletSummaryAsync(int userId);
    Task<WalletDto?> GetWalletAsync(int userId, int currencyId);
}
