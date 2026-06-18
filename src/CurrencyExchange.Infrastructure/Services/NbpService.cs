using System.Text.Json;
using CurrencyExchange.Core.DTOs.ExchangeRate;
using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Interfaces.Repositories;
using CurrencyExchange.Core.Interfaces.Services;

namespace CurrencyExchange.Infrastructure.Services;

public class NbpService : INbpService
{
    private readonly HttpClient _http;
    private readonly IExchangeRateRepository _rateRepo;
    private readonly ICurrencyRepository     _currencyRepo;

    private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };
    private const string NbpBaseUrl = "https://api.nbp.pl/api/exchangerates";

    public NbpService(HttpClient http, IExchangeRateRepository rateRepo, ICurrencyRepository currencyRepo)
    {
        _http         = http;
        _rateRepo     = rateRepo;
        _currencyRepo = currencyRepo;
    }

    public async Task<IEnumerable<ExchangeRateDto>> GetCurrentRatesAsync()
    {
        var result  = new List<ExchangeRateDto>();
        var tables  = await FetchTableCAsync("today");
        var tablesA = await FetchTableAAsync("today");

        foreach (var table in tablesA)
        {
            foreach (var rate in table.Rates)
            {
                var currency = await _currencyRepo.GetByCodeAsync(rate.Code);
                if (currency is null) continue;

                var cTable = tables.SelectMany(t => t.Rates).FirstOrDefault(r => r.Code == rate.Code);
                result.Add(new ExchangeRateDto(
                    0, currency.Id, rate.Code,
                    cTable?.Bid ?? rate.Mid * 0.99m,
                    cTable?.Ask ?? rate.Mid * 1.01m,
                    rate.Mid,
                    DateTime.Parse(table.EffectiveDate),
                    table.No));
            }
        }
        return result;
    }

    public async Task<ExchangeRateDto?> GetCurrentRateAsync(string currencyCode)
    {
        var url  = $"{NbpBaseUrl}/rates/a/{currencyCode.ToLower()}/today/?format=json";
        var resp = await _http.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return null;

        var json = await resp.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<NbpSingleRateResponse>(json, _jsonOpts);
        if (data?.Rates == null || data.Rates.Count == 0) return null;

        var rate     = data.Rates[0];
        var currency = await _currencyRepo.GetByCodeAsync(currencyCode);
        if (currency is null) return null;

        return new ExchangeRateDto(0, currency.Id, currencyCode.ToUpper(),
            rate.Mid * 0.99m, rate.Mid * 1.01m, rate.Mid,
            DateTime.Parse(rate.EffectiveDate), data.No);
    }

    public async Task<IEnumerable<ExchangeRateDto>> GetHistoricalRatesAsync(string currencyCode, DateTime from, DateTime to)
    {
        var fromStr  = from.ToString("yyyy-MM-dd");
        var toStr    = to.ToString("yyyy-MM-dd");
        var url      = $"{NbpBaseUrl}/rates/a/{currencyCode.ToLower()}/{fromStr}/{toStr}/?format=json";
        var resp     = await _http.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return Enumerable.Empty<ExchangeRateDto>();

        var json     = await resp.Content.ReadAsStringAsync();
        var data     = JsonSerializer.Deserialize<NbpSingleRateResponse>(json, _jsonOpts);
        var currency = await _currencyRepo.GetByCodeAsync(currencyCode);
        if (currency is null || data?.Rates == null) return Enumerable.Empty<ExchangeRateDto>();

        return data.Rates.Select(r => new ExchangeRateDto(0, currency.Id, currencyCode.ToUpper(),
            r.Mid * 0.99m, r.Mid * 1.01m, r.Mid, DateTime.Parse(r.EffectiveDate), data.No));
    }

    public async Task SyncRatesAsync()
    {
        var currencies = await _currencyRepo.GetActiveCurrenciesAsync();
        foreach (var currency in currencies.Where(c => c.Code != "PLN"))
        {
            var rateDto = await GetCurrentRateAsync(currency.Code);
            if (rateDto is null) continue;

            var existing = await _rateRepo.GetLatestRateAsync(currency.Id);
            if (existing?.EffectiveDate.Date == rateDto.EffectiveDate.Date) continue;

            await _rateRepo.AddAsync(new ExchangeRate
            {
                CurrencyId      = currency.Id,
                BidRate         = rateDto.BidRate,
                AskRate         = rateDto.AskRate,
                MidRate         = rateDto.MidRate,
                EffectiveDate   = rateDto.EffectiveDate,
                NbpTableNumber  = rateDto.NbpTableNumber
            });
        }
    }

    private async Task<List<NbpTableDto>> FetchTableAAsync(string period)
    {
        var url  = $"{NbpBaseUrl}/tables/a/{period}/?format=json";
        var resp = await _http.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return new List<NbpTableDto>();
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<NbpTableDto>>(json, _jsonOpts) ?? new List<NbpTableDto>();
    }

    private async Task<List<NbpTableCDto>> FetchTableCAsync(string period)
    {
        var url  = $"{NbpBaseUrl}/tables/c/{period}/?format=json";
        var resp = await _http.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return new List<NbpTableCDto>();
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<NbpTableCDto>>(json, _jsonOpts) ?? new List<NbpTableCDto>();
    }

    // Helper response shapes for single-currency endpoint
    private record NbpSingleRateResponse(string Table, string Currency, string Code, string No, List<NbpSingleRate> Rates);
    private record NbpSingleRate(string No, string EffectiveDate, decimal Mid);
}
