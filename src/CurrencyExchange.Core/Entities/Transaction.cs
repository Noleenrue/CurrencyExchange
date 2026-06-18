using CurrencyExchange.Core.Enums;

namespace CurrencyExchange.Core.Entities;

public class Transaction
{
    public int             Id              { get; set; }
    public int             UserId          { get; set; }
    public int             CurrencyId      { get; set; }
    public decimal         Amount          { get; set; }
    public decimal         ExchangeRate    { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime        TransactionDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User     User     { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
}
