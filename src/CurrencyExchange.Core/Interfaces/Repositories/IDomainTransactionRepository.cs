using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Enums;

namespace CurrencyExchange.Core.Interfaces.Repositories;

public interface IDomainTransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Transaction>> GetByCurrencyIdAsync(int currencyId);
    Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type);
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<Transaction?> GetWithDetailsAsync(int id);  // includes User + Currency nav props
}
