using CurrencyExchange.Core.DTOs.Exchange;

namespace CurrencyExchange.Core.Interfaces.Services;

/// <summary>
/// Orchestrates currency exchange operations (buy / sell) with balance validation,
/// NBP rate fetching, wallet updates, and transaction recording — all in one atomic operation.
/// </summary>
public interface ICurrencyExchangeService
{
    /// <summary>
    /// Buys the requested amount of foreign currency.
    /// Deducts PLN from the user's PLN wallet using the current NBP ask rate,
    /// adds the foreign amount to the target wallet, and records the transaction.
    /// </summary>
    /// <param name="request">Buy parameters (userId, currencyCode, amount).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ExchangeResult> BuyCurrencyAsync(BuyCurrencyRequest request, CancellationToken ct = default);

    /// <summary>
    /// Sells the requested amount of foreign currency.
    /// Deducts the foreign amount from the user's foreign wallet using the current NBP bid rate,
    /// adds PLN equivalent to the PLN wallet, and records the transaction.
    /// </summary>
    /// <param name="request">Sell parameters (userId, currencyCode, amount).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ExchangeResult> SellCurrencyAsync(SellCurrencyRequest request, CancellationToken ct = default);
}
