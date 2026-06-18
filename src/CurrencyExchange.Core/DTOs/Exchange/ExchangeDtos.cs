using System.ComponentModel.DataAnnotations;

namespace CurrencyExchange.Core.DTOs.Exchange;

// ─── Requests ────────────────────────────────────────────────────────────────

public class BuyCurrencyRequest
{
    [Required(ErrorMessage = "UserId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "CurrencyCode is required.")]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "CurrencyCode must be 2–10 characters.")]
    public string CurrencyCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.000001, (double)decimal.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
}

public class SellCurrencyRequest
{
    [Required(ErrorMessage = "UserId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "CurrencyCode is required.")]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "CurrencyCode must be 2–10 characters.")]
    public string CurrencyCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.000001, (double)decimal.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
}

// ─── Results ─────────────────────────────────────────────────────────────────

public record ExchangeResult(
    bool    Success,
    string  Message,
    int     TransactionId,
    decimal Amount,
    decimal ExchangeRate,
    decimal PlnEquivalent,
    string  CurrencyCode,
    decimal NewPlnBalance,
    decimal NewForeignBalance,
    DateTime ExecutedAt
);
