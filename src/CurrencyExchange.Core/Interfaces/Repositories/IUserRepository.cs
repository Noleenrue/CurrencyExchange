using CurrencyExchange.Core.Entities;

namespace CurrencyExchange.Core.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdentityIdAsync(string identityId);
}
