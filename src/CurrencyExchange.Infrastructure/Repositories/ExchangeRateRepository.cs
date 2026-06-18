using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Interfaces.Repositories;
using CurrencyExchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Infrastructure.Repositories;

public class ExchangeRateRepository : Repository<ExchangeRate>, IExchangeRateRepository
{
    public ExchangeRateRepository(ApplicationDbContext context) : base(context) { }

    public async Task<ExchangeRate?> GetLatestRateAsync(int currencyId)
        => await _dbSet
            .Where(r => r.CurrencyId == currencyId)
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<ExchangeRate>> GetRateHistoryAsync(int currencyId, DateTime from, DateTime to)
        => await _dbSet
            .Where(r => r.CurrencyId == currencyId && r.EffectiveDate >= from && r.EffectiveDate <= to)
            .OrderBy(r => r.EffectiveDate)
            .ToListAsync();
}
