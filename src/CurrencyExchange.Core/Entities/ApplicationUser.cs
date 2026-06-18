using Microsoft.AspNetCore.Identity;

namespace CurrencyExchange.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ExchangeTransaction> Transactions { get; set; } = new List<ExchangeTransaction>();
    public ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
}
