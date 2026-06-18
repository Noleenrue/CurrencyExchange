namespace CurrencyExchange.Blazor.Services;

/// <summary>
/// Simple in-memory implementation for server-side Blazor.
/// Replace with Blazored.LocalStorage for a proper browser-based implementation.
/// </summary>
public class InMemoryLocalStorageService : ILocalStorageService
{
    private readonly Dictionary<string, string> _store = new();

    public Task SetItemAsync(string key, string value)
    {
        _store[key] = value;
        return Task.CompletedTask;
    }

    public Task<string?> GetItemAsync(string key)
        => Task.FromResult(_store.TryGetValue(key, out var v) ? v : null);

    public Task RemoveItemAsync(string key)
    {
        _store.Remove(key);
        return Task.CompletedTask;
    }
}
