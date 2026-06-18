using CurrencyExchange.Core.Enums;

namespace CurrencyExchange.Core.DTOs.Transaction;

public class CreateTransactionDto
{
    public int             CurrencyId { get; set; }
    public TransactionType Type       { get; set; } = TransactionType.Buy;
    public decimal         Amount     { get; set; }
    public string?         Notes      { get; set; }
}

public record TransactionDto(
    int Id,
    string UserId,
    int CurrencyId,
    string CurrencyCode,
    TransactionType Type,
    decimal Amount,
    decimal ExchangeRate,
    decimal PlnAmount,
    DateTime TransactionDate,
    string? Notes
);

public class TransactionFilterDto
{
    public DateTime?       From       { get; set; }
    public DateTime?       To         { get; set; }
    public int?            CurrencyId { get; set; }
    public TransactionType? Type      { get; set; }
}

