using System.ComponentModel.DataAnnotations;

namespace CurrencyExchange.Core.DTOs.Currency;

public record CurrencyDto(
    int    Id,
    string Code,
    string Name,
    string Symbol,
    bool   IsActive
);

public class CreateCurrencyDto
{
    [Required(ErrorMessage = "Currency code is required.")]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Code must be 2–10 characters.")]
    [RegularExpression("^[A-Za-z]+$", ErrorMessage = "Code must contain only letters.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Currency name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Symbol is required.")]
    [MaxLength(10, ErrorMessage = "Symbol cannot exceed 10 characters.")]
    public string Symbol { get; set; } = string.Empty;
}

public class UpdateCurrencyDto
{
    [Required(ErrorMessage = "Currency name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Symbol is required.")]
    [MaxLength(10, ErrorMessage = "Symbol cannot exceed 10 characters.")]
    public string Symbol { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

