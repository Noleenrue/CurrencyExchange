namespace CurrencyExchange.Core.Entities;

public class Wallet
{
    public int      Id         { get; set; }
    public int      UserId     { get; set; }        // FK → User.Id
    public int      CurrencyId { get; set; }        // FK → Currency.Id
    public decimal  Balance    { get; set; } = 0;

    // Navigation properties
    public User     User     { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
}
