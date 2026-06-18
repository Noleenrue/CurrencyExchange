namespace CurrencyExchange.Core;

/// <summary>
/// Centralised role name constants shared between API and any consumers.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string User  = "User";

    /// <summary>All valid role names.</summary>
    public static readonly IReadOnlyList<string> All = new[] { Admin, User };
}
