using CurrencyExchange.Core.Interfaces.Repositories;

namespace CurrencyExchange.Core.Interfaces;

/// <summary>
/// Coordinates multiple repository operations within a single database transaction.
/// All changes are flushed atomically when <see cref="CommitAsync"/> is called.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository              Users        { get; }
    IWalletRepository            Wallets      { get; }
    ICurrencyRepository          Currencies   { get; }
    IDomainTransactionRepository Transactions { get; }

    /// <summary>Begins an explicit database transaction.</summary>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>Commits the current transaction. Throws if no transaction is active.</summary>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>Rolls back the current transaction and swallows exceptions.</summary>
    Task RollbackAsync();

    /// <summary>Saves all pending changes to the database (without committing the transaction).</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
