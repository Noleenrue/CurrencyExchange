using System.Text;
using System.Text.Json;
using CurrencyExchange.Core.DTOs.DomainTransaction;
using CurrencyExchange.Core.DTOs.Transaction;
using CurrencyExchange.Core.Enums;

namespace CurrencyExchange.Blazor.Services;

public class ApiTransactionService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };

    public ApiTransactionService(HttpClient http) => _http = http;

    // ── Legacy ExchangeTransaction flow ──────────────────────────────────────

    public async Task<List<TransactionDto>> GetMyTransactionsAsync()
    {
        var response = await _http.GetAsync("api/transactions");
        if (!response.IsSuccessStatusCode) return new();
        return JsonSerializer.Deserialize<List<TransactionDto>>(
            await response.Content.ReadAsStringAsync(), _opts) ?? new();
    }

    public async Task<(bool Success, string? Error)> ExecuteAsync(CreateTransactionDto dto)
    {
        var response = await _http.PostAsync("api/transactions",
            new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));
        if (response.IsSuccessStatusCode) return (true, null);
        var body = await response.Content.ReadAsStringAsync();
        return (false, body);
    }

    // ── Domain Transaction history ────────────────────────────────────────────

    /// <summary>Returns all domain transactions for a specific user.</summary>
    public async Task<List<DomainTransactionDto>> GetDomainByUserAsync(int userId)
    {
        var response = await _http.GetAsync($"api/domaintransactions/user/{userId}");
        if (!response.IsSuccessStatusCode) return new();
        return JsonSerializer.Deserialize<List<DomainTransactionDto>>(
            await response.Content.ReadAsStringAsync(), _opts) ?? new();
    }

    /// <summary>Returns all domain transactions.</summary>
    public async Task<List<DomainTransactionDto>> GetAllDomainAsync()
    {
        var response = await _http.GetAsync("api/domaintransactions");
        if (!response.IsSuccessStatusCode) return new();
        return JsonSerializer.Deserialize<List<DomainTransactionDto>>(
            await response.Content.ReadAsStringAsync(), _opts) ?? new();
    }

    /// <summary>Returns domain transactions filtered by type.</summary>
    public async Task<List<DomainTransactionDto>> GetDomainByTypeAsync(TransactionType type)
    {
        var response = await _http.GetAsync($"api/domaintransactions/type/{type}");
        if (!response.IsSuccessStatusCode) return new();
        return JsonSerializer.Deserialize<List<DomainTransactionDto>>(
            await response.Content.ReadAsStringAsync(), _opts) ?? new();
    }

    /// <summary>Returns domain transactions within a date range.</summary>
    public async Task<List<DomainTransactionDto>> GetDomainByDateRangeAsync(DateTime from, DateTime to)
    {
        var url = $"api/domaintransactions/range?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new();
        return JsonSerializer.Deserialize<List<DomainTransactionDto>>(
            await response.Content.ReadAsStringAsync(), _opts) ?? new();
    }
}

