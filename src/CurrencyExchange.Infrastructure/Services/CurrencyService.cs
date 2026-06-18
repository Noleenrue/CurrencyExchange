using CurrencyExchange.Core.DTOs.Currency;
using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Interfaces.Repositories;
using CurrencyExchange.Core.Interfaces.Services;

namespace CurrencyExchange.Infrastructure.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _repo;

    public CurrencyService(ICurrencyRepository repo) => _repo = repo;

    public async Task<IEnumerable<CurrencyDto>> GetAllAsync()
        => (await _repo.GetAllAsync()).Select(Map);

    public async Task<IEnumerable<CurrencyDto>> GetActiveAsync()
        => (await _repo.GetActiveCurrenciesAsync()).Select(Map);

    public async Task<CurrencyDto?> GetByIdAsync(int id)
    {
        var c = await _repo.GetByIdAsync(id);
        return c is null ? null : Map(c);
    }

    public async Task<CurrencyDto> CreateAsync(CreateCurrencyDto dto)
    {
        var code = dto.Code.ToUpper().Trim();
        if (await _repo.ExistsAsync(c => c.Code == code))
            throw new InvalidOperationException($"Currency {code} already exists.");

        var currency = new Currency { Code = code, Name = dto.Name.Trim(), Symbol = dto.Symbol.Trim() };
        await _repo.AddAsync(currency);
        return Map(currency);
    }

    public async Task UpdateAsync(int id, UpdateCurrencyDto dto)
    {
        var currency = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Currency not found.");
        currency.Name     = dto.Name.Trim();
        currency.Symbol   = dto.Symbol.Trim();
        currency.IsActive = dto.IsActive;
        await _repo.UpdateAsync(currency);
    }

    public async Task DeleteAsync(int id)
    {
        var currency = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Currency not found.");
        await _repo.DeleteAsync(currency);
    }

    private static CurrencyDto Map(Currency c)
        => new(c.Id, c.Code, c.Name, c.Symbol, c.IsActive);
}
