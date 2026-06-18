namespace CurrencyExchange.Core.DTOs.Nbp;

// ─── Models returned to callers ──────────────────────────────────────────────

/// <summary>Single exchange rate entry (mid/bid/ask) for one currency on one date.</summary>
public record NbpExchangeRateModel
{
    public string   CurrencyCode   { get; init; } = string.Empty;
    public string   CurrencyName   { get; init; } = string.Empty;
    public decimal  MidRate        { get; init; }
    public decimal? BidRate        { get; init; }   // only in Table C
    public decimal? AskRate        { get; init; }   // only in Table C
    public DateTime EffectiveDate  { get; init; }
    public string   TableNumber    { get; init; } = string.Empty;
    public string   TableType      { get; init; } = string.Empty;   // "A" or "C"
}

/// <summary>All exchange rates fetched in one NBP call (one or more tables).</summary>
public record NbpRatesResponse
{
    public IReadOnlyList<NbpExchangeRateModel> Rates { get; init; }
        = Array.Empty<NbpExchangeRateModel>();

    public DateTime FetchedAt { get; init; } = DateTime.UtcNow;
}

// ─── Raw NBP API JSON shapes ──────────────────────────────────────────────────

/// <summary>NBP /rates/{table}/{code} single-currency response.</summary>
public record NbpSingleCurrencyResponse
{
    public string  Table    { get; init; } = string.Empty;
    public string  Currency { get; init; } = string.Empty;
    public string  Code     { get; init; } = string.Empty;
    public string  No       { get; init; } = string.Empty;
    public List<NbpRateEntry> Rates { get; init; } = new();
}

public record NbpRateEntry
{
    public string  No            { get; init; } = string.Empty;
    public string  EffectiveDate { get; init; } = string.Empty;
    public decimal Mid           { get; init; }
}

/// <summary>NBP /rates/c/{code} bid/ask response entry.</summary>
public record NbpRateEntryC
{
    public string  No            { get; init; } = string.Empty;
    public string  EffectiveDate { get; init; } = string.Empty;
    public decimal Bid           { get; init; }
    public decimal Ask           { get; init; }
}

public record NbpSingleCurrencyResponseC
{
    public string  Table    { get; init; } = string.Empty;
    public string  Currency { get; init; } = string.Empty;
    public string  Code     { get; init; } = string.Empty;
    public string  No       { get; init; } = string.Empty;
    public List<NbpRateEntryC> Rates { get; init; } = new();
}

/// <summary>NBP /tables/{table} full-table response (array wrapper).</summary>
public record NbpTableResponse
{
    public string  Table         { get; init; } = string.Empty;
    public string  No            { get; init; } = string.Empty;
    public string  EffectiveDate { get; init; } = string.Empty;
    public List<NbpTableRateEntry> Rates { get; init; } = new();
}

public record NbpTableRateEntry
{
    public string  Currency { get; init; } = string.Empty;
    public string  Code     { get; init; } = string.Empty;
    public decimal Mid      { get; init; }
}
