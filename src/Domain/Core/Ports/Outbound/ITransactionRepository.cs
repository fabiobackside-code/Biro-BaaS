using Domain.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Ports.Outbound
{
    public interface ITransactionRepository
    {
        Task CreateTransactionAsync(Transaction transaction);
        Task<Transaction> GetTransactionByIdAsync(Guid transactionId);
        Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(Guid accountId);
        Task UpdateTransactionAsync(Transaction transaction);
    }
}
