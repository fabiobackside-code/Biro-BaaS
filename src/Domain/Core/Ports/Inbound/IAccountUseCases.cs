using Domain.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Ports.Inbound
{
    public interface IAccountUseCases
    {
        Task CreateAccountAsync(Account account);
        Task<Account> GetAccountByIdAsync(Guid accountId);
        Task<decimal> GetAccountBalanceAsync(Guid accountId);
        Task<IEnumerable<Transaction>> GetAccountStatementAsync(Guid accountId);
    }
}
