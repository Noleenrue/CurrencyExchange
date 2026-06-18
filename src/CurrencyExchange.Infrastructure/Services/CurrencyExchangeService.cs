using CurrencyExchange.Core.DTOs.Exchange;
using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Enums;
using CurrencyExchange.Core.Interfaces;
using CurrencyExchange.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
namespace CurrencyExchange.Infrastructure.Services;

/// <summary>
/// Orchestrates buy/sell currency exchange operations:
/// fetches the live NBP rate, validates balances, updates wallets,
/// and records every operation in the Transaction table — all inside one DB transaction.
/// </summary>
public class CurrencyExchangeService : ICurrencyExchangeService
{
    /// <summary>Seeded PLN currency ID (see CurrencyExchangeDbContext.SeedData).</summary>
    private const int PlnCurrencyId = 1;

    private readonly IUnitOfWork              _uow;
    private readonly INbpExchangeRateService  _nbp;
    private readonly ILogger<CurrencyExchangeService> _logger;

    public CurrencyExchangeService(
        IUnitOfWork              uow,
        INbpExchangeRateService  nbp,
        ILogger<CurrencyExchangeService> logger)
    {
        _uow    = uow;
        _nbp    = nbp;
        _logger = logger;
    }

    // ─── Buy ──────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<ExchangeResult> BuyCurrencyAsync(
        BuyCurrencyRequest request,
        CancellationToken  ct = default)
    {
        var code = request.CurrencyCode.Trim().ToUpperInvariant();

        // 1. Fetch live NBP rate (use ask rate — what the user pays)
        var rate = await FetchRateAsync(code);
        var useRate = rate.AskRate ?? rate.MidRate;   // Table C ask preferred; Table A mid as fallback
        var plnCost = Math.Round(request.Amount * useRate, 2);

        // 2. Resolve currency entity
        var currency = await _uow.Currencies.GetByCodeAsync(code)
            ?? throw new InvalidOperationException($"Currency '{code}' is not supported.");

        // 3. Validate wallets exist and balances are sufficient
        var plnWallet     = await RequireWalletAsync(request.UserId, PlnCurrencyId, "PLN");
        var foreignWallet = await _uow.Wallets.GetOrCreateWalletAsync(request.UserId, currency.Id);

        ValidateSufficientBalance(plnWallet.Balance, plnCost, "PLN");

        // 4. Execute within a single DB transaction
        await _uow.BeginTransactionAsync(ct);
        try
        {
            plnWallet.Balance     -= plnCost;
            foreignWallet.Balance += request.Amount;

            await _uow.Wallets.UpdateAsync(plnWallet);
            await _uow.Wallets.UpdateAsync(foreignWallet);

            var tx = new Transaction
            {
                UserId          = request.UserId,
                CurrencyId      = currency.Id,
                Amount          = request.Amount,
                ExchangeRate    = useRate,
                TransactionType = TransactionType.Buy,
                TransactionDate = DateTime.UtcNow
            };
            await _uow.Transactions.AddAsync(tx);

            await _uow.CommitAsync(ct);

            _logger.LogInformation(
                "BUY {Amount} {Code} @ {Rate} for user {UserId}. PLN deducted: {Pln}",
                request.Amount, code, useRate, request.UserId, plnCost);

            return new ExchangeResult(
                Success:            true,
                Message:            $"Successfully bought {request.Amount} {code} for {plnCost} PLN.",
                TransactionId:      tx.Id,
                Amount:             request.Amount,
                ExchangeRate:       useRate,
                PlnEquivalent:      plnCost,
                CurrencyCode:       code,
                NewPlnBalance:      plnWallet.Balance,
                NewForeignBalance:  foreignWallet.Balance,
                ExecutedAt:         tx.TransactionDate
            );
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }
    }

    // ─── Sell ─────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<ExchangeResult> SellCurrencyAsync(
        SellCurrencyRequest request,
        CancellationToken   ct = default)
    {
        var code = request.CurrencyCode.Trim().ToUpperInvariant();

        // 1. Fetch live NBP rate (use bid rate — what the user receives)
        var rate = await FetchRateAsync(code);
        var useRate  = rate.BidRate ?? rate.MidRate;  // Table C bid preferred; Table A mid as fallback
        var plnGain  = Math.Round(request.Amount * useRate, 2);

        // 2. Resolve currency entity
        var currency = await _uow.Currencies.GetByCodeAsync(code)
            ?? throw new InvalidOperationException($"Currency '{code}' is not supported.");

        // 3. Validate wallets exist and balances are sufficient
        var foreignWallet = await RequireWalletAsync(request.UserId, currency.Id, code);
        var plnWallet     = await _uow.Wallets.GetOrCreateWalletAsync(request.UserId, PlnCurrencyId);

        ValidateSufficientBalance(foreignWallet.Balance, request.Amount, code);

        // 4. Execute within a single DB transaction
        await _uow.BeginTransactionAsync(ct);
        try
        {
            foreignWallet.Balance -= request.Amount;
            plnWallet.Balance     += plnGain;

            await _uow.Wallets.UpdateAsync(foreignWallet);
            await _uow.Wallets.UpdateAsync(plnWallet);

            var tx = new Transaction
            {
                UserId          = request.UserId,
                CurrencyId      = currency.Id,
                Amount          = request.Amount,
                ExchangeRate    = useRate,
                TransactionType = TransactionType.Sell,
                TransactionDate = DateTime.UtcNow
            };
            await _uow.Transactions.AddAsync(tx);

            await _uow.CommitAsync(ct);

            _logger.LogInformation(
                "SELL {Amount} {Code} @ {Rate} for user {UserId}. PLN received: {Pln}",
                request.Amount, code, useRate, request.UserId, plnGain);

            return new ExchangeResult(
                Success:            true,
                Message:            $"Successfully sold {request.Amount} {code} for {plnGain} PLN.",
                TransactionId:      tx.Id,
                Amount:             request.Amount,
                ExchangeRate:       useRate,
                PlnEquivalent:      plnGain,
                CurrencyCode:       code,
                NewPlnBalance:      plnWallet.Balance,
                NewForeignBalance:  foreignWallet.Balance,
                ExecutedAt:         tx.TransactionDate
            );
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    /// <summary>Fetches current NBP rate; throws if the currency is not quoted today.</summary>
    private async Task<Core.DTOs.Nbp.NbpExchangeRateModel> FetchRateAsync(string code)
    {
        var rate = await _nbp.GetCurrentRateAsync(code);

        if (rate is null)
            throw new InvalidOperationException(
                $"No NBP rate available for '{code}' today. " +
                $"Markets may be closed or the currency is not quoted.");

        return rate;
    }

    /// <summary>Fetches an existing wallet; throws <see cref="KeyNotFoundException"/> if absent.</summary>
    private async Task<Wallet> RequireWalletAsync(int userId, int currencyId, string currencyLabel)
    {
        var wallet = await _uow.Wallets.GetUserWalletAsync(userId, currencyId);

        if (wallet is null)
            throw new KeyNotFoundException(
                $"User {userId} does not have a {currencyLabel} wallet. " +
                $"Please create one before performing exchange operations.");

        return wallet;
    }

    /// <summary>Throws <see cref="InvalidOperationException"/> if balance is insufficient.</summary>
    private static void ValidateSufficientBalance(decimal balance, decimal required, string currency)
    {
        if (balance < required)
            throw new InvalidOperationException(
                $"Insufficient {currency} balance. " +
                $"Available: {balance:F2}, Required: {required:F2}.");
    }
}
