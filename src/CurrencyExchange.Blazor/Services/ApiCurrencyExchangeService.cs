using System.Text;
using System.Text.Json;
using CurrencyExchange.Core.DTOs.Exchange;

namespace CurrencyExchange.Blazor.Services;

public class ApiCurrencyExchangeService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };

    public ApiCurrencyExchangeService(HttpClient http) => _http = http;

    public async Task<(bool Success, ExchangeResult? Result, string? Error)> BuyAsync(BuyCurrencyRequest req)
    {
        var json     = new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("api/currencyexchange/buy", json);
        var body     = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var result = JsonSerializer.Deserialize<ExchangeResult>(body, _opts);
            return (true, result, null);
        }

        var err = TryExtractMessage(body);
        return (false, null, err);
    }

    public async Task<(bool Success, ExchangeResult? Result, string? Error)> SellAsync(SellCurrencyRequest req)
    {
        var json     = new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("api/currencyexchange/sell", json);
        var body     = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var result = JsonSerializer.Deserialize<ExchangeResult>(body, _opts);
            return (true, result, null);
        }

        var err = TryExtractMessage(body);
        return (false, null, err);
    }

    private static string TryExtractMessage(string body)
    {
        try
        {
            var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("message", out var msg))
                return msg.GetString() ?? body;
        }
        catch { }
        return body;
    }
}
