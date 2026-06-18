namespace CurrencyExchange.Core.Entities;

public class ExchangeRate
{
    public int Id { get; set; }
    public int CurrencyId { get; set; }
    public decimal BidRate { get; set; }    // Buy rate (bank buys foreign currency)
    public decimal AskRate { get; set; }    // Sell rate (bank sells foreign currency)
    public decimal MidRate { get; set; }    // Mid/reference rate from NBP
    public DateTime EffectiveDate { get; set; }
    public string? NbpTableNumber { get; set; }

    public Currency Currency { get; set; } = null!;
}
