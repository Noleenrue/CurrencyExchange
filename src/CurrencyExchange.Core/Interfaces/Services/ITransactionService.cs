using CurrencyExchange.Core.DTOs.Transaction;

namespace CurrencyExchange.Core.Interfaces.Services;

public interface ITransactionService
{
    Task<TransactionDto> ExecuteTransactionAsync(string userId, CreateTransactionDto dto);
    Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(string userId, TransactionFilterDto? filter = null);
    Task<TransactionDto?> GetByIdAsync(int id);
}
