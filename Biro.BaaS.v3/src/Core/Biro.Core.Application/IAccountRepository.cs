using Biro.Core.Domain.Entities;

namespace Biro.Core.Application.Repositories;

public interface IAccountRepository
{
    Task<Account> GetByIdAsync(Guid id);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
}
