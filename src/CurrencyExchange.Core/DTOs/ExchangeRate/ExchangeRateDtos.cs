namespace CurrencyExchange.Core.DTOs.ExchangeRate;

public record ExchangeRateDto(
    int Id,
    int CurrencyId,
    string CurrencyCode,
    decimal BidRate,
    decimal AskRate,
    decimal MidRate,
    DateTime EffectiveDate,
    string? NbpTableNumber
);

// NBP API response shapes
public record NbpTableDto(
    string Table,
    string No,
    string EffectiveDate,
    List<NbpRateDto> Rates
);

public record NbpRateDto(
    string Currency,
    string Code,
    decimal Mid
);

public record NbpTableCDto(
    string Table,
    string No,
    string EffectiveDate,
    List<NbpRateCDto> Rates
);

public record NbpRateCDto(
    string Currency,
    string Code,
    decimal Bid,
    decimal Ask
);
