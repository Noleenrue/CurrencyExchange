using System.ComponentModel.DataAnnotations;

namespace CurrencyExchange.Core.DTOs.Wallet;

public record WalletDto(
    int      Id,
    int?     CurrencyId,
    string   CurrencyCode,
    string   CurrencyName,
    decimal  Balance
);

public record WalletSummaryDto(
    decimal                PlnBalance,
    IEnumerable<WalletDto> ForeignBalances
);

public class CreateWalletDto
{
    [Required(ErrorMessage = "UserId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "CurrencyId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "CurrencyId must be a positive integer.")]
    public int CurrencyId { get; set; }

    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Initial balance cannot be negative.")]
    public decimal InitialBalance { get; set; } = 0;
}

public class UpdateWalletDto
{
    [Required(ErrorMessage = "Balance is required.")]
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Balance cannot be negative.")]
    public decimal Balance { get; set; }
}

