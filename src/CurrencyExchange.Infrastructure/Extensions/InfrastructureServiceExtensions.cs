using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Interfaces;
using CurrencyExchange.Core.Interfaces.Repositories;
using CurrencyExchange.Core.Interfaces.Services;
using CurrencyExchange.Infrastructure.Data;
using CurrencyExchange.Infrastructure.Repositories;
using CurrencyExchange.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyExchange.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connString = configuration.GetConnectionString("DefaultConnection");

        // Identity DbContext (ASP.NET Identity tables + ExchangeTransactions)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connString));

        // Domain DbContext (Users, Currencies, Wallets, Transactions)
        services.AddDbContext<CurrencyExchangeDbContext>(options =>
            options.UseSqlServer(connString));

        // Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit           = true;
            options.Password.RequireLowercase        = true;
            options.Password.RequireUppercase        = true;
            options.Password.RequireNonAlphanumeric  = false;
            options.Password.RequiredLength          = 8;
            options.User.RequireUniqueEmail           = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
        // RoleManager is automatically registered by AddIdentity

        // Repositories (specific registrations; no open-generic due to dual-context setup)
        services.AddScoped<ICurrencyRepository,           CurrencyRepository>();
        services.AddScoped<ITransactionRepository,        TransactionRepository>();
        services.AddScoped<IWalletRepository,             WalletRepository>();
        services.AddScoped<IExchangeRateRepository,       ExchangeRateRepository>();
        services.AddScoped<IUserRepository,               UserRepository>();
        services.AddScoped<IDomainTransactionRepository,  DomainTransactionRepository>();

        // Services
        services.AddScoped<IAuthService,               AuthService>();
        services.AddScoped<ICurrencyService,           CurrencyService>();
        services.AddScoped<ITransactionService,        TransactionService>();
        services.AddScoped<IWalletService,             WalletService>();
        services.AddScoped<INbpService,                NbpService>();
        services.AddScoped<INbpExchangeRateService,    NbpExchangeRateService>();
        services.AddScoped<ICurrencyExchangeService,   CurrencyExchangeService>();

        // Unit of Work (scoped — one per HTTP request, shares CurrencyExchangeDbContext)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // HTTP client for legacy NbpService
        services.AddHttpClient<NbpService>(client =>
        {
            client.BaseAddress = new Uri("https://api.nbp.pl/api/exchangerates/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // HTTP client for NbpExchangeRateService
        services.AddHttpClient<NbpExchangeRateService>(client =>
        {
            client.BaseAddress = new Uri("https://api.nbp.pl/api/exchangerates/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
