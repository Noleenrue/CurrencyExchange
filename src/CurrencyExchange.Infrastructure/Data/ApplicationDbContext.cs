using CurrencyExchange.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Currency>          Currencies   => Set<Currency>();
    public DbSet<ExchangeTransaction> Transactions => Set<ExchangeTransaction>();
    public DbSet<ExchangeRate>      ExchangeRates => Set<ExchangeRate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Exclude domain entities that belong to CurrencyExchangeDbContext only
        builder.Ignore<User>();
        builder.Ignore<Wallet>();
        builder.Ignore<Transaction>();

        builder.Entity<Currency>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.Code).IsUnique();
            e.Property(c => c.Code).HasMaxLength(10).IsRequired();
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.Property(c => c.Symbol).HasMaxLength(10).IsRequired();
            // Ignore nav props that live in CurrencyExchangeDbContext
            e.Ignore(c => c.Transactions);
            e.Ignore(c => c.Wallets);
        });

        builder.Entity<ExchangeTransaction>(e =>
        {
            e.ToTable("ExchangeTransactions");   // explicit name avoids clash with domain Transactions table
            e.HasKey(t => t.Id);
            e.Property(t => t.Amount).HasPrecision(18, 6);
            e.Property(t => t.ExchangeRate).HasPrecision(18, 6);
            e.Property(t => t.PlnAmount).HasPrecision(18, 2);
            e.HasOne(t => t.User)
             .WithMany(u => u.Transactions)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Currency)
             .WithMany(c => c.ExchangeTransactions)
             .HasForeignKey(t => t.CurrencyId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ExchangeRate>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.BidRate).HasPrecision(18, 6);
            e.Property(r => r.AskRate).HasPrecision(18, 6);
            e.Property(r => r.MidRate).HasPrecision(18, 6);
            e.HasOne(r => r.Currency)
             .WithMany(c => c.ExchangeRates)
             .HasForeignKey(r => r.CurrencyId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Currency>().HasData(
            new Currency { Id = 1, Code = "PLN", Name = "Polish Złoty",  Symbol = "zł", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Currency { Id = 2, Code = "USD", Name = "US Dollar",     Symbol = "$",  IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Currency { Id = 3, Code = "EUR", Name = "Euro",          Symbol = "€",  IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Currency { Id = 4, Code = "GBP", Name = "British Pound", Symbol = "£",  IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Currency { Id = 5, Code = "CHF", Name = "Swiss Franc",   Symbol = "Fr", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
