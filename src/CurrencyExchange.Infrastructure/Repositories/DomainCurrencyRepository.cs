using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Interfaces.Repositories;
using CurrencyExchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Infrastructure.Repositories;

/// <summary>
/// Currency repository backed by <see cref="CurrencyExchangeDbContext"/>.
/// Used by <see cref="UnitOfWork"/> so that all domain operations share one context.
/// </summary>
public class DomainCurrencyRepository : Repository<Currency>, ICurrencyRepository
{
    public DomainCurrencyRepository(CurrencyExchangeDbContext context) : base(context) { }

    public async Task<Currency?> GetByCodeAsync(string code)
        => await _dbSet.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant());

    public async Task<IEnumerable<Currency>> GetActiveCurrenciesAsync()
        => await _dbSet.Where(c => c.IsActive).OrderBy(c => c.Code).ToListAsync();
}
