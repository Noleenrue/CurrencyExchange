using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CurrencyExchange.Core;
using CurrencyExchange.Core.DTOs.Auth;
using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CurrencyExchange.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser>  _userManager;
    private readonly RoleManager<IdentityRole>     _roleManager;
    private readonly IConfiguration                _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole>    roleManager,
        IConfiguration               configuration)
    {
        _userManager   = userManager;
        _roleManager   = roleManager;
        _configuration = configuration;
    }

    // ── Register ──────────────────────────────────────────────────────────────

    public async Task<LoginResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            throw new InvalidOperationException("Passwords do not match.");

        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            throw new InvalidOperationException("Email already registered.");

        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName  = dto.LastName,
            Email     = dto.Email,
            UserName  = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));

        // Every new user gets the "User" role
        await EnsureRoleExistsAsync(Roles.User);
        await _userManager.AddToRoleAsync(user, Roles.User);

        return await GenerateTokenAsync(user);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return await GenerateTokenAsync(user);
    }

    // ── Profile ───────────────────────────────────────────────────────────────

    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return new UserProfileDto(
            user.Id, user.FirstName, user.LastName,
            user.Email!, user.PhoneNumber, user.CreatedAt);
    }

    public async Task UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        user.FirstName   = dto.FirstName;
        user.LastName    = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;

        await _userManager.UpdateAsync(user);
    }

    // ── Role management ───────────────────────────────────────────────────────

    public async Task AssignRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        if (!Roles.All.Contains(role))
            throw new InvalidOperationException($"Role '{role}' is not a valid role. Valid: {string.Join(", ", Roles.All)}");

        await EnsureRoleExistsAsync(role);

        if (!await _userManager.IsInRoleAsync(user, role))
            await _userManager.AddToRoleAsync(user, role);
    }

    // ── JWT token generation ──────────────────────────────────────────────────

    private async Task<LoginResponseDto> GenerateTokenAsync(ApplicationUser user)
    {
        var jwtSettings   = _configuration.GetSection("JwtSettings");
        var secretKey     = jwtSettings["SecretKey"]!;
        var issuer        = jwtSettings["Issuer"]!;
        var audience      = jwtSettings["Audience"]!;
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier,     user.Id),
            new(ClaimTypes.Email,              user.Email!),
            new("firstName",                   user.FirstName),
            new("lastName",                    user.LastName),
        };

        // Add one ClaimTypes.Role claim per role so [Authorize(Roles="Admin")] works
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires:           expiry,
            signingCredentials: creds);

        return new LoginResponseDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Email!, user.FirstName, user.LastName,
            expiry, roles.ToList());
    }

    private async Task EnsureRoleExistsAsync(string role)
    {
        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole(role));
    }
}

