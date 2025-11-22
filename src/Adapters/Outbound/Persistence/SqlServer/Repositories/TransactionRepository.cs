using Domain.Core.Entities;
using Domain.Core.Ports.Outbound;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Adapters.Outbound.Persistence.SqlServer.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly IDbConnection _connection;
        private readonly Func<IDbTransaction> _transactionFactory;

        public TransactionRepository(IDbConnection connection, Func<IDbTransaction> transactionFactory)
        {
            _connection = connection;
            _transactionFactory = transactionFactory;
        }

        public async Task CreateTransactionAsync(Transaction transaction)
        {
            var sql = "INSERT INTO Transactions (TransactionId, AccountId, TransactionType, Amount, Timestamp, CorrelationId, Status, ExpirationDateTime, Metadata) VALUES (@TransactionId, @AccountId, @TransactionType, @Amount, @Timestamp, @CorrelationId, @Status, @ExpirationDateTime, @Metadata)";
            await _connection.ExecuteAsync(sql, transaction, _transactionFactory());
        }

        public async Task<Transaction> GetTransactionByIdAsync(Guid transactionId)
        {
            var sql = "SELECT * FROM Transactions WHERE TransactionId = @TransactionId";
            return await _connection.QuerySingleOrDefaultAsync<Transaction>(sql, new { TransactionId = transactionId }, _transactionFactory());
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(Guid accountId)
        {
            var sql = "SELECT * FROM Transactions WHERE AccountId = @AccountId ORDER BY Timestamp DESC";
            return await _connection.QueryAsync<Transaction>(sql, new { AccountId = accountId }, _transactionFactory());
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            var sql = "UPDATE Transactions SET Status = @Status WHERE TransactionId = @TransactionId";
            await _connection.ExecuteAsync(sql, transaction, _transactionFactory());
        }
    }
}
