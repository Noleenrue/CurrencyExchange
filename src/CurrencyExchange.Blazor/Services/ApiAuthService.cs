using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CurrencyExchange.Core.DTOs.Auth;
using CurrencyExchange.Core.DTOs.User;

namespace CurrencyExchange.Blazor.Services;

public class ApiAuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _storage;
    private static readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };

    public ApiAuthService(HttpClient http, ILocalStorageService storage)
    {
        _http    = http;
        _storage = storage;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var response = await _http.PostAsync("api/auth/login",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode) return null;

        var result = JsonSerializer.Deserialize<LoginResponseDto>(
            await response.Content.ReadAsStringAsync(), _opts);

        if (result is not null)
        {
            await _storage.SetItemAsync("authToken", result.Token);
            await _storage.SetItemAsync("authEmail", result.Email);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
        }
        return result;
    }

    public async Task<bool> RegisterAsync(RegisterDto dto)
    {
        var response = await _http.PostAsync("api/auth/register",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Registers and, on success, stores the returned token and returns it for auth state notification.
    /// </summary>
    public async Task<(bool Success, string? Token, string? Error)> RegisterAndGetTokenAsync(RegisterDto dto)
    {
        var response = await _http.PostAsync("api/auth/register",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            var msg = TryExtractMessage(body);
            return (false, null, msg);
        }

        var result = JsonSerializer.Deserialize<LoginResponseDto>(body, _opts);
        if (result is null) return (false, null, "Unexpected server response.");

        await _storage.SetItemAsync("authToken", result.Token);
        await _storage.SetItemAsync("authEmail", result.Email);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
        return (true, result.Token, null);
    }

    private static string TryExtractMessage(string body)
    {
        try
        {
            var doc = System.Text.Json.JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("message", out var msg))
                return msg.GetString() ?? body;
        }
        catch { }
        return body;
    }

    public async Task LogoutAsync()
    {
        await _storage.RemoveItemAsync("authToken");
        await _storage.RemoveItemAsync("authEmail");
        await _storage.RemoveItemAsync("domainUserId");
        _http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<UserProfileDto?> GetProfileAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _http.GetAsync("api/auth/profile");
        if (!response.IsSuccessStatusCode) return null;
        return JsonSerializer.Deserialize<UserProfileDto>(
            await response.Content.ReadAsStringAsync(), _opts);
    }

    /// <summary>
    /// Returns the int domain User.Id by looking up the email stored at login.
    /// Result is cached in local storage for the session.
    /// </summary>
    public async Task<int?> GetDomainUserIdAsync()
    {
        var cached = await _storage.GetItemAsync("domainUserId");
        if (int.TryParse(cached, out var cachedId)) return cachedId;

        var email = await _storage.GetItemAsync("authEmail");
        if (string.IsNullOrEmpty(email)) return null;

        await SetAuthHeaderAsync();
        var response = await _http.GetAsync($"api/users/by-email?email={Uri.EscapeDataString(email)}");
        if (!response.IsSuccessStatusCode) return null;

        var user = JsonSerializer.Deserialize<UserDto>(
            await response.Content.ReadAsStringAsync(), _opts);
        if (user is null) return null;

        await _storage.SetItemAsync("domainUserId", user.Id.ToString());
        return user.Id;
    }

    public async Task SetAuthHeaderAsync()
    {
        var token = await _storage.GetItemAsync("authToken");
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _storage.GetItemAsync("authToken");
        return !string.IsNullOrEmpty(token);
    }
}

// Simple local storage abstraction (inject ILocalStorageService from Blazored.LocalStorage or implement)
public interface ILocalStorageService
{
    Task SetItemAsync(string key, string value);
    Task<string?> GetItemAsync(string key);
    Task RemoveItemAsync(string key);
}

