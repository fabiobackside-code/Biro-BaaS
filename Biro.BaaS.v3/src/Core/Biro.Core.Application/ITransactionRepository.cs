using Biro.Core.Domain.Entities;

namespace Biro.Core.Application.Repositories;

public interface ITransactionRepository
{
    Task<Transaction> GetByIdAsync(Guid id);
    Task AddAsync(Transaction transaction);
}
