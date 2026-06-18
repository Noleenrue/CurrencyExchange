namespace CurrencyExchange.Core.Exceptions;

/// <summary>
/// Thrown when the NBP API returns an unexpected response or is unreachable.
/// </summary>
public class NbpApiException : Exception
{
    /// <summary>HTTP status code returned by NBP (0 = network/timeout error).</summary>
    public int StatusCode { get; }

    /// <summary>Raw response body from NBP, if available.</summary>
    public string? ResponseBody { get; }

    public NbpApiException(string message, int statusCode, string? responseBody, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode   = statusCode;
        ResponseBody = responseBody;
    }
}
