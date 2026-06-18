using CurrencyExchange.Core.Entities;

namespace CurrencyExchange.Core.Interfaces.Repositories;

public interface IExchangeRateRepository : IRepository<ExchangeRate>
{
    Task<ExchangeRate?> GetLatestRateAsync(int currencyId);
    Task<IEnumerable<ExchangeRate>> GetRateHistoryAsync(int currencyId, DateTime from, DateTime to);
}
