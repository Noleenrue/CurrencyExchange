using CurrencyExchange.Core.DTOs.Nbp;

namespace CurrencyExchange.Core.Interfaces.Services;

/// <summary>
/// Dedicated service for fetching exchange rates from the NBP (National Bank of Poland) API.
/// Docs: https://api.nbp.pl/
/// </summary>
public interface INbpExchangeRateService
{
    /// <summary>
    /// Gets the current (today's) exchange rate for the given currency code
    /// using NBP Table A (mid rate) and Table C (bid/ask rates).
    /// Returns null when the currency is not quoted today (e.g. weekends/holidays).
    /// </summary>
    /// <param name="currencyCode">ISO 4217 code, e.g. "USD", "EUR".</param>
    Task<NbpExchangeRateModel?> GetCurrentRateAsync(string currencyCode);

    /// <summary>
    /// Gets all currency rates from today's NBP Table A (mid rates).
    /// </summary>
    Task<NbpRatesResponse> GetAllCurrentRatesAsync();

    /// <summary>
    /// Gets the exchange rate for a specific date.
    /// Returns null when no rate is published for the given date.
    /// </summary>
    /// <param name="currencyCode">ISO 4217 code, e.g. "USD".</param>
    /// <param name="date">The date to query. Must not be in the future.</param>
    Task<NbpExchangeRateModel?> GetRateByDateAsync(string currencyCode, DateOnly date);

    /// <summary>
    /// Gets historical exchange rates for a currency within a date range.
    /// NBP allows a maximum window of 93 days per request.
    /// </summary>
    /// <param name="currencyCode">ISO 4217 code, e.g. "USD".</param>
    /// <param name="from">Start date (inclusive).</param>
    /// <param name="to">End date (inclusive).</param>
    Task<NbpRatesResponse> GetHistoricalRatesAsync(string currencyCode, DateOnly from, DateOnly to);

    /// <summary>
    /// Gets historical exchange rates for a currency within a date range,
    /// automatically splitting into 93-day chunks if the range exceeds the NBP limit.
    /// </summary>
    Task<NbpRatesResponse> GetHistoricalRatesUnlimitedAsync(string currencyCode, DateOnly from, DateOnly to);
}
