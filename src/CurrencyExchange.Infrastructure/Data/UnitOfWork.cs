using CurrencyExchange.Core.Interfaces;
using CurrencyExchange.Core.Interfaces.Repositories;
using CurrencyExchange.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace CurrencyExchange.Infrastructure.Data;

/// <summary>
/// Coordinates multiple repository operations within a single EF Core transaction.
/// All repositories share the same <see cref="CurrencyExchangeDbContext"/> instance so
/// multiple SaveChangesAsync calls inside a transaction are all rolled back together
/// if CommitAsync is never reached.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CurrencyExchangeDbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy-initialised repositories sharing the same context
    private IUserRepository?              _users;
    private IWalletRepository?            _wallets;
    private ICurrencyRepository?          _currencies;
    private IDomainTransactionRepository? _transactions;

    public UnitOfWork(CurrencyExchangeDbContext context) => _context = context;

    // ─── Repository accessors ─────────────────────────────────────────────────

    public IUserRepository              Users        => _users        ??= new UserRepository(_context);
    public IWalletRepository            Wallets      => _wallets      ??= new WalletRepository(_context);
    public ICurrencyRepository          Currencies   => _currencies   ??= new DomainCurrencyRepository(_context);
    public IDomainTransactionRepository Transactions => _transactions ??= new DomainTransactionRepository(_context);

    // ─── Transaction management ───────────────────────────────────────────────

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("A transaction is already active. Commit or roll back before starting a new one.");

        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction to commit.");

        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    /// <inheritdoc />
    public async Task RollbackAsync()
    {
        if (_transaction is null) return;

        try   { await _transaction.RollbackAsync(); }
        catch { /* best-effort: connection may already be broken */ }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    // ─── IAsyncDisposable ─────────────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
            await RollbackAsync();   // auto-rollback any uncommitted transaction

        await _context.DisposeAsync();
    }
}
