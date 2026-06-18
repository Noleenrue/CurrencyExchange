using CurrencyExchange.Core.DTOs.ExchangeRate;

namespace CurrencyExchange.Core.Interfaces.Services;

public interface INbpService
{
    Task<IEnumerable<ExchangeRateDto>> GetCurrentRatesAsync();
    Task<ExchangeRateDto?> GetCurrentRateAsync(string currencyCode);
    Task<IEnumerable<ExchangeRateDto>> GetHistoricalRatesAsync(string currencyCode, DateTime from, DateTime to);
    Task SyncRatesAsync();  // Fetch from NBP and persist to DB
}
