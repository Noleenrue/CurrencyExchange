using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Enums;
using CurrencyExchange.Core.Interfaces.Repositories;
using CurrencyExchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Infrastructure.Repositories;

public class TransactionRepository : Repository<ExchangeTransaction>, ITransactionRepository
{
    public TransactionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<ExchangeTransaction>> GetUserTransactionsAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        int? currencyId = null,
        TransactionType? type = null)
    {
        var query = _dbSet
            .Include(t => t.Currency)
            .Where(t => t.UserId == userId);

        if (from.HasValue)       query = query.Where(t => t.TransactionDate >= from.Value);
        if (to.HasValue)         query = query.Where(t => t.TransactionDate <= to.Value);
        if (currencyId.HasValue) query = query.Where(t => t.CurrencyId == currencyId.Value);
        if (type.HasValue)       query = query.Where(t => t.Type == type.Value);

        return await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
    }
}
