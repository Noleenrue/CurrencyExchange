using CurrencyExchange.Core.Enums;

namespace CurrencyExchange.Core.Entities;

public class ExchangeTransaction
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CurrencyId { get; set; }
    public TransactionType Type { get; set; }           // Buy or Sell
    public decimal Amount { get; set; }                 // Amount of foreign currency
    public decimal ExchangeRate { get; set; }           // Rate used at transaction time
    public decimal PlnAmount { get; set; }              // PLN equivalent
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
}
