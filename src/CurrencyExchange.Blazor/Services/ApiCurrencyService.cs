using System.Text;
using System.Text.Json;
using CurrencyExchange.Core.DTOs.Currency;

namespace CurrencyExchange.Blazor.Services;

public class ApiCurrencyService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };

    public ApiCurrencyService(HttpClient http) => _http = http;

    public async Task<List<CurrencyDto>> GetActiveAsync()
    {
        var response = await _http.GetAsync("api/currencies/active");
        if (!response.IsSuccessStatusCode) return new();
        return JsonSerializer.Deserialize<List<CurrencyDto>>(
            await response.Content.ReadAsStringAsync(), _opts) ?? new();
    }

    public async Task<List<CurrencyDto>> GetAllAsync()
    {
        var response = await _http.GetAsync("api/currencies");
        if (!response.IsSuccessStatusCode) return new();
        return JsonSerializer.Deserialize<List<CurrencyDto>>(
            await response.Content.ReadAsStringAsync(), _opts) ?? new();
    }

    public async Task<bool> CreateAsync(CreateCurrencyDto dto)
    {
        var json     = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("api/currencies", json);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, UpdateCurrencyDto dto)
    {
        var json     = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
        var response = await _http.PutAsync($"api/currencies/{id}", json);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/currencies/{id}");
        return response.IsSuccessStatusCode;
    }
}

