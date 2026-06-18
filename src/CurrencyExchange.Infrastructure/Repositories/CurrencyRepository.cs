using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Interfaces.Repositories;
using CurrencyExchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Infrastructure.Repositories;

public class CurrencyRepository : Repository<Currency>, ICurrencyRepository
{
    public CurrencyRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Currency?> GetByCodeAsync(string code)
        => await _dbSet.FirstOrDefaultAsync(c => c.Code == code.ToUpper());

    public async Task<IEnumerable<Currency>> GetActiveCurrenciesAsync()
        => await _dbSet.Where(c => c.IsActive).OrderBy(c => c.Code).ToListAsync();
}
