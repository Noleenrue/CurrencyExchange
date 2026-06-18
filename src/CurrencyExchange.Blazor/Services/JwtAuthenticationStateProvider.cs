using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace CurrencyExchange.Blazor.Services;

/// <summary>
/// Blazor Server authentication state provider that reads the JWT token
/// stored in <see cref="ILocalStorageService"/>, parses claims (including roles)
/// and reports auth state to the Blazor component tree.
/// </summary>
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly ILocalStorageService _storage;

    public JwtAuthenticationStateProvider(ILocalStorageService storage)
        => _storage = storage;

    // ── AuthenticationStateProvider contract ─────────────────────────────────

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _storage.GetItemAsync("authToken");
        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        var claims = ParseClaimsFromJwt(token);
        if (claims is null)
            return Anonymous;

        // Reject expired tokens
        var exp = claims.FirstOrDefault(c => c.Type == "exp");
        if (exp is not null &&
            long.TryParse(exp.Value, out var expUnix) &&
            DateTimeOffset.UtcNow > DateTimeOffset.FromUnixTimeSeconds(expUnix))
        {
            await _storage.RemoveItemAsync("authToken");
            return Anonymous;
        }

        var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Email, ClaimTypes.Role);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    // ── Notification helpers called by login/logout ───────────────────────────

    /// <summary>Call after a successful login to push the new state to all subscribers.</summary>
    public void NotifyUserAuthenticated(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        if (claims is null) { NotifyUserLoggedOut(); return; }

        var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Email, ClaimTypes.Role);
        var user     = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    /// <summary>Call after logout to clear the auth state.</summary>
    public void NotifyUserLoggedOut()
        => NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));

    // ── JWT parsing (no external NuGet needed — pure base64 decode) ───────────

    private static IEnumerable<Claim>? ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length != 3) return null;

            // Base64Url → Base64
            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            payload = (payload.Length % 4) switch
            {
                2 => payload + "==",
                3 => payload + "=",
                _ => payload
            };

            var bytes  = Convert.FromBase64String(payload);
            var json   = System.Text.Encoding.UTF8.GetString(bytes);
            var kvPairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (kvPairs is null) return null;

            var claims = new List<Claim>();
            foreach (var (key, element) in kvPairs)
            {
                // Map short JWT claim names to .NET ClaimTypes where appropriate
                var claimType = key switch
                {
                    "sub"   => ClaimTypes.NameIdentifier,
                    "email" => ClaimTypes.Email,
                    "role"  => ClaimTypes.Role,
                    _       => key
                };

                if (element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                        claims.Add(new Claim(claimType, item.GetString() ?? string.Empty));
                }
                else
                {
                    claims.Add(new Claim(claimType, element.ToString()));
                }
            }
            return claims;
        }
        catch
        {
            return null;
        }
    }
}
