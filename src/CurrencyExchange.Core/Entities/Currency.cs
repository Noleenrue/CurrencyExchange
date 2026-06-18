namespace CurrencyExchange.Core.Entities;

public class Currency
{
    public int    Id        { get; set; }
    public string Code      { get; set; } = string.Empty;   // e.g. USD, EUR
    public string Name      { get; set; } = string.Empty;   // e.g. US Dollar
    public string Symbol    { get; set; } = string.Empty;   // e.g. $
    public bool   IsActive  { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ExchangeTransaction> ExchangeTransactions { get; set; } = new List<ExchangeTransaction>();
    public ICollection<Transaction>         Transactions         { get; set; } = new List<Transaction>();
    public ICollection<Wallet>              Wallets              { get; set; } = new List<Wallet>();
    public ICollection<ExchangeRate>        ExchangeRates        { get; set; } = new List<ExchangeRate>();
}
