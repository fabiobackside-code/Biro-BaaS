using Domain.Core.Entities;
using System.Threading.Tasks;

namespace Domain.Core.Ports.Outbound
{
    public interface IAccountRepository
    {
        Task CreateAccountAsync(Account account);
        Task<Account> GetAccountByIdAsync(Guid accountId);
        Task<Account> GetAccountByNumberAsync(string branchCode, string accountNumber);
        Task<decimal> GetAccountBalanceAsync(Guid accountId);
        Task UpdateAccountAsync(Account account);
    }
}
