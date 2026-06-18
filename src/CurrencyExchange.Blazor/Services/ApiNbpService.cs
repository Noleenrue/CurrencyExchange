using System.Text.Json;
using CurrencyExchange.Core.DTOs.Nbp;

namespace CurrencyExchange.Blazor.Services;

public class ApiNbpService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };

    public ApiNbpService(HttpClient http) => _http = http;

    /// <summary>Returns today's rates for all currencies (NBP Table A).</summary>
    public async Task<NbpRatesResponse?> GetAllCurrentRatesAsync()
    {
        var response = await _http.GetAsync("api/nbpexchangerate/current/all");
        if (!response.IsSuccessStatusCode) return null;
        return JsonSerializer.Deserialize<NbpRatesResponse>(
            await response.Content.ReadAsStringAsync(), _opts);
    }

    /// <summary>Returns current rate for a single currency code.</summary>
    public async Task<NbpExchangeRateModel?> GetCurrentRateAsync(string code)
    {
        var response = await _http.GetAsync($"api/nbpexchangerate/current/{Uri.EscapeDataString(code)}");
        if (!response.IsSuccessStatusCode) return null;
        return JsonSerializer.Deserialize<NbpExchangeRateModel>(
            await response.Content.ReadAsStringAsync(), _opts);
    }

    /// <summary>Returns historical rates for a currency within a date range.</summary>
    public async Task<NbpRatesResponse?> GetHistoricalAsync(string code, DateOnly from, DateOnly to)
    {
        var url = $"api/nbpexchangerate/historical/{Uri.EscapeDataString(code)}" +
                  $"?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return JsonSerializer.Deserialize<NbpRatesResponse>(
            await response.Content.ReadAsStringAsync(), _opts);
    }
}
