namespace CurrencyExchange.Core.Entities;

public class User
{
    public int     Id           { get; set; }
    public string  FullName     { get; set; } = string.Empty;
    public string  Email        { get; set; } = string.Empty;
    public string  PasswordHash { get; set; } = string.Empty;

    /// <summary>Links this domain user to the ASP.NET Identity user (string GUID).</summary>
    public string? IdentityId   { get; set; }

    // Navigation properties
    public ICollection<Wallet>      Wallets      { get; set; } = new List<Wallet>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
