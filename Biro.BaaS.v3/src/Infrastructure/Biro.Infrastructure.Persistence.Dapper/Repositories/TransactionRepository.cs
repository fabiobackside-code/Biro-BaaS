using Biro.Core.Application.Repositories;
using Biro.Core.Domain.Entities;
using Dapper;

namespace Biro.Infrastructure.Persistence.Dapper.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TransactionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Transaction> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM transactions WHERE id = @id";
        return await connection.QueryFirstOrDefaultAsync<Transaction>(sql, new { id });
    }

    public async Task AddAsync(Transaction transaction)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        const string sql = "INSERT INTO transactions (id, account_id, transaction_type, amount, created_at) VALUES (@Id, @AccountId, @TransactionType, @Amount, @CreatedAt)";
        await connection.ExecuteAsync(sql, transaction);
    }
}
