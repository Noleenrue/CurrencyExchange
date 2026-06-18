using System.Net;
using System.Text.Json;
using CurrencyExchange.Core.DTOs.Nbp;
using CurrencyExchange.Core.Exceptions;
using CurrencyExchange.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CurrencyExchange.Infrastructure.Services;

/// <summary>
/// Fetches exchange rates from the National Bank of Poland (NBP) public API.
/// Base URL: https://api.nbp.pl/api/exchangerates/
///
/// Key endpoints used:
///   GET /rates/a/{code}/                       → today's mid rate
///   GET /rates/c/{code}/                       → today's bid/ask rate
///   GET /tables/a/                             → all mid rates today
///   GET /rates/a/{code}/{date}/                → rate on specific date
///   GET /rates/a/{code}/{startDate}/{endDate}/ → historical range (max 93 days)
/// </summary>
public class NbpExchangeRateService : INbpExchangeRateService
{
    private readonly HttpClient _http;
    private readonly ILogger<NbpExchangeRateService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // NBP enforces a hard maximum of 93 days per range query
    private const int NbpMaxRangeDays = 93;
    private const string DateFormat   = "yyyy-MM-dd";

    public NbpExchangeRateService(HttpClient http, ILogger<NbpExchangeRateService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    // ─── Public API ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<NbpExchangeRateModel?> GetCurrentRateAsync(string currencyCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
        var code = currencyCode.ToUpperInvariant();

        _logger.LogDebug("Fetching current NBP rate for {Code}", code);

        // Fetch Table A (mid) and Table C (bid/ask) in parallel
        var midTask    = FetchSingleRateTableAAsync(code, "today");
        var bidAskTask = FetchSingleRateTableCAsync(code, "today");
        await Task.WhenAll(midTask, bidAskTask);

        var midEntry    = midTask.Result;
        var bidAskEntry = bidAskTask.Result;

        if (midEntry is null)
        {
            _logger.LogWarning("No NBP rate published today for {Code}", code);
            return null;
        }

        return new NbpExchangeRateModel
        {
            CurrencyCode  = midEntry.Code.ToUpperInvariant(),
            CurrencyName  = midEntry.Currency,
            MidRate       = midEntry.Rates[0].Mid,
            BidRate       = bidAskEntry?.Rates[0].Bid,
            AskRate       = bidAskEntry?.Rates[0].Ask,
            EffectiveDate = ParseDate(midEntry.Rates[0].EffectiveDate),
            TableNumber   = midEntry.No,
            TableType     = midEntry.Table
        };
    }

    /// <inheritdoc/>
    public async Task<NbpRatesResponse> GetAllCurrentRatesAsync()
    {
        _logger.LogDebug("Fetching all current NBP rates (Table A)");

        var tables = await FetchFullTableAAsync("today");
        var rates  = tables
            .SelectMany(t => t.Rates.Select(r => new NbpExchangeRateModel
            {
                CurrencyCode  = r.Code.ToUpperInvariant(),
                CurrencyName  = r.Currency,
                MidRate       = r.Mid,
                EffectiveDate = ParseDate(t.EffectiveDate),
                TableNumber   = t.No,
                TableType     = t.Table
            }))
            .ToList();

        return new NbpRatesResponse { Rates = rates };
    }

    /// <inheritdoc/>
    public async Task<NbpExchangeRateModel?> GetRateByDateAsync(string currencyCode, DateOnly date)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
        if (date > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentOutOfRangeException(nameof(date), "Cannot query future exchange rates.");

        var code    = currencyCode.ToUpperInvariant();
        var dateStr = date.ToString(DateFormat);

        _logger.LogDebug("Fetching NBP rate for {Code} on {Date}", code, dateStr);

        var midEntry = await FetchSingleRateTableAAsync(code, dateStr);
        if (midEntry is null || midEntry.Rates.Count == 0) return null;

        // Try bid/ask on the same date
        var bidAskEntry = await FetchSingleRateTableCAsync(code, dateStr);
        var rate        = midEntry.Rates[0];

        return new NbpExchangeRateModel
        {
            CurrencyCode  = midEntry.Code.ToUpperInvariant(),
            CurrencyName  = midEntry.Currency,
            MidRate       = rate.Mid,
            BidRate       = bidAskEntry?.Rates.FirstOrDefault()?.Bid,
            AskRate       = bidAskEntry?.Rates.FirstOrDefault()?.Ask,
            EffectiveDate = ParseDate(rate.EffectiveDate),
            TableNumber   = rate.No,
            TableType     = midEntry.Table
        };
    }

    /// <inheritdoc/>
    public async Task<NbpRatesResponse> GetHistoricalRatesAsync(string currencyCode, DateOnly from, DateOnly to)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
        ValidateDateRange(from, to);

        var totalDays = to.DayNumber - from.DayNumber;
        if (totalDays > NbpMaxRangeDays)
            throw new ArgumentException(
                $"Date range exceeds the NBP maximum of {NbpMaxRangeDays} days. " +
                $"Use {nameof(GetHistoricalRatesUnlimitedAsync)} for longer ranges.");

        var code     = currencyCode.ToUpperInvariant();
        var fromStr  = from.ToString(DateFormat);
        var toStr    = to.ToString(DateFormat);

        _logger.LogDebug("Fetching historical NBP rates for {Code} from {From} to {To}", code, fromStr, toStr);

        var midResponse = await FetchSingleRateTableAAsync(code, $"{fromStr}/{toStr}");
        if (midResponse is null || midResponse.Rates.Count == 0)
            return new NbpRatesResponse();

        var rates = midResponse.Rates
            .Select(r => new NbpExchangeRateModel
            {
                CurrencyCode  = midResponse.Code.ToUpperInvariant(),
                CurrencyName  = midResponse.Currency,
                MidRate       = r.Mid,
                EffectiveDate = ParseDate(r.EffectiveDate),
                TableNumber   = r.No,
                TableType     = midResponse.Table
            })
            .ToList();

        return new NbpRatesResponse { Rates = rates };
    }

    /// <inheritdoc/>
    public async Task<NbpRatesResponse> GetHistoricalRatesUnlimitedAsync(
        string currencyCode, DateOnly from, DateOnly to)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
        ValidateDateRange(from, to);

        var allRates = new List<NbpExchangeRateModel>();
        var current  = from;

        while (current <= to)
        {
            var chunkEnd = current.AddDays(NbpMaxRangeDays - 1);
            if (chunkEnd > to) chunkEnd = to;

            _logger.LogDebug(
                "Fetching NBP rate chunk for {Code}: {From} – {To}",
                currencyCode.ToUpperInvariant(), current, chunkEnd);

            var chunk = await GetHistoricalRatesAsync(currencyCode, current, chunkEnd);
            allRates.AddRange(chunk.Rates);

            current = chunkEnd.AddDays(1);
        }

        return new NbpRatesResponse { Rates = allRates };
    }

    // ─── Private HTTP helpers ─────────────────────────────────────────────────

    /// <summary>
    /// Calls /rates/a/{code}/{period}/ — returns mid rates.
    /// period can be "today", a date string "2024-01-15", or range "2024-01-01/2024-03-31".
    /// Returns null on 404 (no rate published for that period).
    /// </summary>
    private async Task<NbpSingleCurrencyResponse?> FetchSingleRateTableAAsync(
        string code, string period)
    {
        var url = $"rates/a/{code.ToLowerInvariant()}/{period}/?format=json";
        return await GetAsync<NbpSingleCurrencyResponse>(url);
    }

    /// <summary>
    /// Calls /rates/c/{code}/{period}/ — returns bid/ask rates.
    /// Returns null on 404.
    /// </summary>
    private async Task<NbpSingleCurrencyResponseC?> FetchSingleRateTableCAsync(
        string code, string period)
    {
        var url = $"rates/c/{code.ToLowerInvariant()}/{period}/?format=json";
        return await GetAsync<NbpSingleCurrencyResponseC>(url);
    }

    /// <summary>
    /// Calls /tables/a/{period}/ — returns all currencies for a given date.
    /// </summary>
    private async Task<IReadOnlyList<NbpTableResponse>> FetchFullTableAAsync(string period)
    {
        var url      = $"tables/a/{period}/?format=json";
        var response = await GetAsync<List<NbpTableResponse>>(url);
        return response ?? new List<NbpTableResponse>();
    }

    /// <summary>
    /// Generic GET helper with structured error handling.
    /// Returns null on 404; throws <see cref="NbpApiException"/> on other failures.
    /// </summary>
    private async Task<T?> GetAsync<T>(string relativeUrl) where T : class
    {
        try
        {
            var response = await _http.GetAsync(relativeUrl);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogDebug("NBP returned 404 for {Url}", relativeUrl);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "NBP API error {Status} for {Url}: {Body}",
                    (int)response.StatusCode, relativeUrl, body);

                throw new NbpApiException(
                    $"NBP API returned {(int)response.StatusCode} for '{relativeUrl}'.",
                    (int)response.StatusCode, body);
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (NbpApiException)
        {
            throw;  // let domain exceptions propagate as-is
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling NBP API at {Url}", relativeUrl);
            throw new NbpApiException(
                $"Network error while contacting NBP API: {ex.Message}", 0, null, ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout calling NBP API at {Url}", relativeUrl);
            throw new NbpApiException(
                "NBP API request timed out.", 0, null, ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parse error for NBP response at {Url}", relativeUrl);
            throw new NbpApiException(
                $"Failed to parse NBP API response: {ex.Message}", 0, null, ex);
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static DateTime ParseDate(string raw)
        => DateTime.TryParse(raw, out var dt) ? dt : DateTime.MinValue;

    private static void ValidateDateRange(DateOnly from, DateOnly to)
    {
        if (from > to)
            throw new ArgumentException($"'from' ({from}) must not be after 'to' ({to}).");
        if (to > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentOutOfRangeException(nameof(to), "Cannot query future exchange rates.");
    }
}
