using CurrencyExchange.Core.Entities;

namespace CurrencyExchange.Core.Interfaces.Repositories;

public interface ICurrencyRepository : IRepository<Currency>
{
    Task<Currency?> GetByCodeAsync(string code);
    Task<IEnumerable<Currency>> GetActiveCurrenciesAsync();
}
