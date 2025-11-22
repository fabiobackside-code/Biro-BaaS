using Domain.Core.Entities;
using Domain.Core.Ports.Inbound;
using Domain.Core.Ports.Outbound;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class AccountUseCases : IAccountUseCases
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountUseCases(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateAccountAsync(Account account)
        {
            await _unitOfWork.Accounts.CreateAccountAsync(account);
        }

        public async Task<Account> GetAccountByIdAsync(System.Guid accountId)
        {
            return await _unitOfWork.Accounts.GetAccountByIdAsync(accountId);
        }

        public async Task<decimal> GetAccountBalanceAsync(System.Guid accountId)
        {
            return await _unitOfWork.Accounts.GetAccountBalanceAsync(accountId);
        }

        public async Task<IEnumerable<Transaction>> GetAccountStatementAsync(System.Guid accountId)
        {
            return await _unitOfWork.Transactions.GetTransactionsByAccountIdAsync(accountId);
        }
    }
}
