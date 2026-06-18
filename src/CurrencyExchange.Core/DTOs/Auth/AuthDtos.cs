using System.ComponentModel.DataAnnotations;

namespace CurrencyExchange.Core.DTOs.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(100)]
    public string FirstName       { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(100)]
    public string LastName        { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email           { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string Password        { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email    { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Returned after a successful login or registration.</summary>
public record LoginResponseDto(
    string       Token,
    string       Email,
    string       FirstName,
    string       LastName,
    DateTime     Expiry,
    List<string> Roles
);

public record UserProfileDto(
    string    Id,
    string    FirstName,
    string    LastName,
    string    Email,
    string?   PhoneNumber,
    DateTime  CreatedAt
);

public class UpdateProfileDto
{
    [Required] [MaxLength(100)]
    public string FirstName    { get; set; } = string.Empty;

    [Required] [MaxLength(100)]
    public string LastName     { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }
}

public class AssignRoleDto
{
    [Required(ErrorMessage = "UserId is required.")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    public string Role   { get; set; } = string.Empty;
}


