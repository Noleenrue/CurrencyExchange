using CurrencyExchange.Core.DTOs.Auth;

namespace CurrencyExchange.Core.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto> RegisterAsync(RegisterDto dto);
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task<UserProfileDto>   GetProfileAsync(string userId);
    Task                   UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task                   AssignRoleAsync(string userId, string role);
}
