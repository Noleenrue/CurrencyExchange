using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Enums;
using CurrencyExchange.Core.Interfaces.Repositories;
using CurrencyExchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Infrastructure.Repositories;

public class DomainTransactionRepository : Repository<Transaction>, IDomainTransactionRepository
{
    public DomainTransactionRepository(CurrencyExchangeDbContext context) : base(context) { }

    public async Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId)
        => await _dbSet
            .Include(t => t.Currency)
            .Include(t => t.User)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

    public async Task<IEnumerable<Transaction>> GetByCurrencyIdAsync(int currencyId)
        => await _dbSet
            .Include(t => t.Currency)
            .Include(t => t.User)
            .Where(t => t.CurrencyId == currencyId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

    public async Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type)
        => await _dbSet
            .Include(t => t.Currency)
            .Include(t => t.User)
            .Where(t => t.TransactionType == type)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime from, DateTime to)
        => await _dbSet
            .Include(t => t.Currency)
            .Include(t => t.User)
            .Where(t => t.TransactionDate >= from && t.TransactionDate <= to)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

    public async Task<Transaction?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(t => t.Currency)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);
}
