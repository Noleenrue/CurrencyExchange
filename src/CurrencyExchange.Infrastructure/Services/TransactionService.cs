using CurrencyExchange.Core.DTOs.Transaction;
using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Enums;
using CurrencyExchange.Core.Interfaces.Repositories;
using CurrencyExchange.Core.Interfaces.Services;

namespace CurrencyExchange.Infrastructure.Services;

public class TransactionService : ITransactionService
{
    private const int PlnCurrencyId = 1;   // matches seeded PLN currency

    private readonly ITransactionRepository  _txRepo;
    private readonly IWalletRepository       _walletRepo;
    private readonly IExchangeRateRepository _rateRepo;
    private readonly ICurrencyRepository     _currencyRepo;
    private readonly IUserRepository         _userRepo;

    public TransactionService(
        ITransactionRepository txRepo,
        IWalletRepository walletRepo,
        IExchangeRateRepository rateRepo,
        ICurrencyRepository currencyRepo,
        IUserRepository userRepo)
    {
        _txRepo       = txRepo;
        _walletRepo   = walletRepo;
        _rateRepo     = rateRepo;
        _currencyRepo = currencyRepo;
        _userRepo     = userRepo;
    }

    public async Task<TransactionDto> ExecuteTransactionAsync(string identityUserId, CreateTransactionDto dto)
    {
        // Resolve domain user from Identity ID
        var domainUser = await _userRepo.GetByIdentityIdAsync(identityUserId)
            ?? throw new KeyNotFoundException("Domain user not found. Please ensure registration is complete.");

        var currency = await _currencyRepo.GetByIdAsync(dto.CurrencyId)
            ?? throw new KeyNotFoundException("Currency not found.");

        var rate = await _rateRepo.GetLatestRateAsync(dto.CurrencyId)
            ?? throw new InvalidOperationException("No exchange rate available for this currency.");

        // Buy = user pays PLN, receives foreign currency
        // Sell = user pays foreign currency, receives PLN
        decimal rateUsed  = dto.Type == TransactionType.Buy ? rate.AskRate : rate.BidRate;
        decimal plnAmount = dto.Amount * rateUsed;

        var plnWallet     = await _walletRepo.GetOrCreateWalletAsync(domainUser.Id, PlnCurrencyId);
        var foreignWallet = await _walletRepo.GetOrCreateWalletAsync(domainUser.Id, dto.CurrencyId);

        if (dto.Type == TransactionType.Buy)
        {
            if (plnWallet.Balance < plnAmount)
                throw new InvalidOperationException("Insufficient PLN balance.");
            plnWallet.Balance     -= plnAmount;
            foreignWallet.Balance += dto.Amount;
        }
        else
        {
            if (foreignWallet.Balance < dto.Amount)
                throw new InvalidOperationException($"Insufficient {currency.Code} balance.");
            foreignWallet.Balance -= dto.Amount;
            plnWallet.Balance     += plnAmount;
        }

        await _walletRepo.UpdateAsync(plnWallet);
        await _walletRepo.UpdateAsync(foreignWallet);

        var transaction = new ExchangeTransaction
        {
            UserId          = identityUserId,
            CurrencyId      = dto.CurrencyId,
            Type            = dto.Type,
            Amount          = dto.Amount,
            ExchangeRate    = rateUsed,
            PlnAmount       = plnAmount,
            TransactionDate = DateTime.UtcNow,
            Notes           = dto.Notes
        };

        await _txRepo.AddAsync(transaction);
        return MapDto(transaction, currency.Code);
    }

    public async Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(string userId, TransactionFilterDto? filter = null)
    {
        var txs = await _txRepo.GetUserTransactionsAsync(
            userId, filter?.From, filter?.To, filter?.CurrencyId, filter?.Type);
        return txs.Select(t => MapDto(t, t.Currency?.Code ?? ""));
    }

    public async Task<TransactionDto?> GetByIdAsync(int id)
    {
        var tx = await _txRepo.GetByIdAsync(id);
        return tx is null ? null : MapDto(tx, tx.Currency?.Code ?? "");
    }

    private static TransactionDto MapDto(ExchangeTransaction t, string currencyCode)
        => new(t.Id, t.UserId, t.CurrencyId, currencyCode, t.Type, t.Amount, t.ExchangeRate, t.PlnAmount, t.TransactionDate, t.Notes);
}
