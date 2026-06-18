using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Enums;

namespace CurrencyExchange.Core.Interfaces.Repositories;

public interface ITransactionRepository : IRepository<ExchangeTransaction>
{
    Task<IEnumerable<ExchangeTransaction>> GetUserTransactionsAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        int? currencyId = null,
        TransactionType? type = null);
}
