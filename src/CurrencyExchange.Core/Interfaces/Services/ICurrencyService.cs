using CurrencyExchange.Core.DTOs.Currency;

namespace CurrencyExchange.Core.Interfaces.Services;

public interface ICurrencyService
{
    Task<IEnumerable<CurrencyDto>> GetAllAsync();
    Task<IEnumerable<CurrencyDto>> GetActiveAsync();
    Task<CurrencyDto?> GetByIdAsync(int id);
    Task<CurrencyDto> CreateAsync(CreateCurrencyDto dto);
    Task UpdateAsync(int id, UpdateCurrencyDto dto);
    Task DeleteAsync(int id);
}
