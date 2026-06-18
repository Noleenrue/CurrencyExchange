using System.Text.Json;
using CurrencyExchange.Core.DTOs.ExchangeRate;
using CurrencyExchange.Core.DTOs.Wallet;

namespace CurrencyExchange.Blazor.Services;

public class ApiWalletService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };

    public ApiWalletService(HttpClient http) => _http = http;

    public async Task<WalletSummaryDto?> GetSummaryAsync()
    {
        var response = await _http.GetAsync("api/wallet");
        if (!response.IsSuccessStatusCode) return null;
        return JsonSerializer.Deserialize<WalletSummaryDto>(
            await response.Content.ReadAsStringAsync(), _opts);
    }

    /// <summary>Returns all wallets for a specific domain user.</summary>
    public async Task<List<WalletDto>> GetByUserAsync(int userId)
    {
        var response = await _http.GetAsync($"api/wallets/user/{userId}");
        if (!response.IsSuccessStatusCode) return new();
        return JsonSerializer.Deserialize<List<WalletDto>>(
            await response.Content.ReadAsStringAsync(), _opts) ?? new();
    }

    /// <summary>Returns all wallets (admin view).</summary>
    public async Task<List<WalletDto>> GetAllAsync()
    {
        var response = await _http.GetAsync("api/wallets");
        if (!response.IsSuccessStatusCode) return new();
        return JsonSerializer.Deserialize<List<WalletDto>>(
            await response.Content.ReadAsStringAsync(), _opts) ?? new();
    }
}

public class ApiExchangeRateService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };

    public ApiExchangeRateService(HttpClient http) => _http = http;

    public async Task<List<ExchangeRateDto>> GetCurrentRatesAsync()
    {
        var response = await _http.GetAsync("api/exchangerates/current");
        if (!response.IsSuccessStatusCode) return new();
        return JsonSerializer.Deserialize<List<ExchangeRateDto>>(
            await response.Content.ReadAsStringAsync(), _opts) ?? new();
    }

    public async Task<List<ExchangeRateDto>> GetHistoricalAsync(string code, DateTime from, DateTime to)
    {
        var url = $"api/exchangerates/historical/{code}?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new();
        return JsonSerializer.Deserialize<List<ExchangeRateDto>>(
            await response.Content.ReadAsStringAsync(), _opts) ?? new();
    }
}

