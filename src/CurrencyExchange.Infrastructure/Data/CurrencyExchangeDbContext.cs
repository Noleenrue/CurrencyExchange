using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Infrastructure.Data;

/// <summary>
/// Domain DbContext for the Currency Exchange system.
/// Uses the simplified User entity (not ASP.NET Identity).
/// </summary>
public class CurrencyExchangeDbContext : DbContext
{
    public CurrencyExchangeDbContext(DbContextOptions<CurrencyExchangeDbContext> options)
        : base(options) { }

    public DbSet<User>        Users        => Set<User>();
    public DbSet<Currency>    Currencies   => Set<Currency>();
    public DbSet<Wallet>      Wallets      => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Exclude Identity-specific entities (they live in ApplicationDbContext)
        modelBuilder.Ignore<ApplicationUser>();
        modelBuilder.Ignore<ExchangeTransaction>();
        modelBuilder.Ignore<ExchangeRate>();

        ConfigureUser(modelBuilder);
        ConfigureCurrency(modelBuilder);
        ConfigureWallet(modelBuilder);
        ConfigureTransaction(modelBuilder);
        SeedData(modelBuilder);
    }

    // ─── User ────────────────────────────────────────────────────────────────

    private static void ConfigureUser(ModelBuilder mb)
    {
        mb.Entity<User>(e =>
        {
            e.ToTable("Users");

            e.HasKey(u => u.Id);

            e.Property(u => u.Id)
             .UseIdentityColumn();

            e.Property(u => u.FullName)
             .HasMaxLength(200)
             .IsRequired();

            e.Property(u => u.Email)
             .HasMaxLength(320)
             .IsRequired();

            e.Property(u => u.PasswordHash)
             .HasMaxLength(500)
             .IsRequired();

            e.Property(u => u.IdentityId)
             .HasMaxLength(450);            // matches ASP.NET Identity key length

            e.HasIndex(u => u.Email)
             .IsUnique()
             .HasDatabaseName("IX_Users_Email");

            e.HasIndex(u => u.IdentityId)
             .HasDatabaseName("IX_Users_IdentityId");

            // One user → many wallets
            e.HasMany(u => u.Wallets)
             .WithOne(w => w.User)
             .HasForeignKey(w => w.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // One user → many transactions
            e.HasMany(u => u.Transactions)
             .WithOne(t => t.User)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ─── Currency ────────────────────────────────────────────────────────────

    private static void ConfigureCurrency(ModelBuilder mb)
    {
        mb.Entity<Currency>(e =>
        {
            e.ToTable("Currencies");

            e.HasKey(c => c.Id);

            e.Property(c => c.Code)
             .HasMaxLength(10)
             .IsRequired()
             .IsUnicode(false);

            e.Property(c => c.Name)
             .HasMaxLength(100)
             .IsRequired();

            e.Property(c => c.Symbol)
             .HasMaxLength(10);

            e.Property(c => c.IsActive)
             .HasDefaultValue(true);

            e.Property(c => c.CreatedAt)
             .HasDefaultValueSql("GETUTCDATE()");

            e.HasIndex(c => c.Code)
             .IsUnique()
             .HasDatabaseName("IX_Currencies_Code");

            // Ignore nav props that live in ApplicationDbContext
            e.Ignore(c => c.ExchangeTransactions);
            e.Ignore(c => c.ExchangeRates);

            // One currency → many wallets
            e.HasMany(c => c.Wallets)
             .WithOne(w => w.Currency)
             .HasForeignKey(w => w.CurrencyId)
             .OnDelete(DeleteBehavior.Restrict);

            // One currency → many transactions
            e.HasMany(c => c.Transactions)
             .WithOne(t => t.Currency)
             .HasForeignKey(t => t.CurrencyId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ─── Wallet ──────────────────────────────────────────────────────────────

    private static void ConfigureWallet(ModelBuilder mb)
    {
        mb.Entity<Wallet>(e =>
        {
            e.ToTable("Wallets");

            e.HasKey(w => w.Id);

            e.Property(w => w.Balance)
             .HasPrecision(18, 6)           // supports crypto-level precision
             .HasDefaultValue(0m)
             .IsRequired();

            // Each user has at most one wallet per currency
            e.HasIndex(w => new { w.UserId, w.CurrencyId })
             .IsUnique()
             .HasDatabaseName("IX_Wallets_UserId_CurrencyId");

            // FK: Wallet → User  (configured on User side above)
            e.HasOne(w => w.User)
             .WithMany(u => u.Wallets)
             .HasForeignKey(w => w.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // FK: Wallet → Currency
            e.HasOne(w => w.Currency)
             .WithMany(c => c.Wallets)
             .HasForeignKey(w => w.CurrencyId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ─── Transaction ─────────────────────────────────────────────────────────

    private static void ConfigureTransaction(ModelBuilder mb)
    {
        mb.Entity<Transaction>(e =>
        {
            e.ToTable("Transactions");

            e.HasKey(t => t.Id);

            e.Property(t => t.Amount)
             .HasPrecision(18, 6)
             .IsRequired();

            e.Property(t => t.ExchangeRate)
             .HasPrecision(18, 6)
             .IsRequired();

            e.Property(t => t.TransactionType)
             .HasConversion<string>()       // stores "Buy" / "Sell" as string
             .HasMaxLength(10)
             .IsRequired();

            e.Property(t => t.TransactionDate)
             .HasDefaultValueSql("GETUTCDATE()")
             .IsRequired();

            // Index for fast user history lookups
            e.HasIndex(t => t.UserId)
             .HasDatabaseName("IX_Transactions_UserId");

            // Index for date-range queries
            e.HasIndex(t => t.TransactionDate)
             .HasDatabaseName("IX_Transactions_TransactionDate");

            // FK: Transaction → User
            e.HasOne(t => t.User)
             .WithMany(u => u.Transactions)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            // FK: Transaction → Currency
            e.HasOne(t => t.Currency)
             .WithMany(c => c.Transactions)
             .HasForeignKey(t => t.CurrencyId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ─── Seed Data ───────────────────────────────────────────────────────────

    private static void SeedData(ModelBuilder mb)
    {
        mb.Entity<Currency>().HasData(
            new Currency { Id = 1, Code = "PLN", Name = "Polish Złoty",  Symbol = "zł", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Currency { Id = 2, Code = "USD", Name = "US Dollar",     Symbol = "$",  IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Currency { Id = 3, Code = "EUR", Name = "Euro",          Symbol = "€",  IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Currency { Id = 4, Code = "GBP", Name = "British Pound", Symbol = "£",  IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Currency { Id = 5, Code = "CHF", Name = "Swiss Franc",   Symbol = "Fr", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
