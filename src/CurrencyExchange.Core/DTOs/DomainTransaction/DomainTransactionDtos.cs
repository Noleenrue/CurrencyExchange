using System.ComponentModel.DataAnnotations;
using CurrencyExchange.Core.Enums;

namespace CurrencyExchange.Core.DTOs.DomainTransaction;

public record DomainTransactionDto(
    int             Id,
    int             UserId,
    int             CurrencyId,
    string          CurrencyCode,
    string          UserFullName,
    decimal         Amount,
    decimal         ExchangeRate,
    TransactionType TransactionType,
    DateTime        TransactionDate
);

public class CreateDomainTransactionDto
{
    [Required(ErrorMessage = "UserId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "CurrencyId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "CurrencyId must be a positive integer.")]
    public int CurrencyId { get; set; }

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.000001, (double)decimal.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "ExchangeRate is required.")]
    [Range(0.000001, (double)decimal.MaxValue, ErrorMessage = "ExchangeRate must be greater than zero.")]
    public decimal ExchangeRate { get; set; }

    [Required(ErrorMessage = "TransactionType is required.")]
    public TransactionType TransactionType { get; set; }
}

public class UpdateDomainTransactionDto
{
    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.000001, (double)decimal.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "ExchangeRate is required.")]
    [Range(0.000001, (double)decimal.MaxValue, ErrorMessage = "ExchangeRate must be greater than zero.")]
    public decimal ExchangeRate { get; set; }

    [Required(ErrorMessage = "TransactionType is required.")]
    public TransactionType TransactionType { get; set; }
}
