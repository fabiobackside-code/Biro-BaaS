using Domain.Core.Entities;
using Domain.Core.Ports.Outbound;
using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Adapters.Outbound.Persistence.SqlServer.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IDbConnection _connection;
        private readonly Func<IDbTransaction> _transactionFactory;

        public AccountRepository(IDbConnection connection, Func<IDbTransaction> transactionFactory)
        {
            _connection = connection;
            _transactionFactory = transactionFactory;
        }

        public async Task CreateAccountAsync(Account account)
        {
            var sql = "INSERT INTO Accounts (AccountId, ClientId, ProductType, BranchCode, AccountNumber, Status, OpenedAt, ClosedAt) VALUES (@AccountId, @ClientId, @ProductType, @BranchCode, @AccountNumber, @Status, @OpenedAt, @ClosedAt)";
            await _connection.ExecuteAsync(sql, account, _transactionFactory());
        }

        public async Task<Account> GetAccountByIdAsync(Guid accountId)
        {
            var sql = "SELECT * FROM Accounts WHERE AccountId = @AccountId";
            return await _connection.QuerySingleOrDefaultAsync<Account>(sql, new { AccountId = accountId }, _transactionFactory());
        }

        public async Task<Account> GetAccountByNumberAsync(string branchCode, string accountNumber)
        {
            var sql = "SELECT * FROM Accounts WHERE BranchCode = @BranchCode AND AccountNumber = @AccountNumber";
            return await _connection.QuerySingleOrDefaultAsync<Account>(sql, new { BranchCode = branchCode, AccountNumber = accountNumber }, _transactionFactory());
        }

        public async Task<decimal> GetAccountBalanceAsync(Guid accountId)
        {
            var sql = "SELECT dbo.fn_GetAvailableBalance(@AccountId)";
            return await _connection.ExecuteScalarAsync<decimal>(sql, new { AccountId = accountId }, _transactionFactory());
        }

        public async Task UpdateAccountAsync(Account account)
        {
            var sql = "UPDATE Accounts SET Status = @Status, ClosedAt = @ClosedAt WHERE AccountId = @AccountId";
            await _connection.ExecuteAsync(sql, account, _transactionFactory());
        }
    }
}
