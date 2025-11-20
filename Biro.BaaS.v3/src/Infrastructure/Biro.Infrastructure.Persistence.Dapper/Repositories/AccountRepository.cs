using Biro.Core.Application.Repositories;
using Biro.Core.Domain.Entities;
using Dapper;

namespace Biro.Infrastructure.Persistence.Dapper.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AccountRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Account> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM accounts WHERE id = @id";
        return await connection.QueryFirstOrDefaultAsync<Account>(sql, new { id });
    }

    public async Task AddAsync(Account account)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        const string sql = "INSERT INTO accounts (id, client_id, account_number, branch_code, product_type, status) VALUES (@Id, @ClientId, @AccountNumber, @BranchCode, @ProductType, @Status)";
        await connection.ExecuteAsync(sql, account);
    }

    public async Task UpdateAsync(Account account)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        const string sql = "UPDATE accounts SET account_number = @AccountNumber, branch_code = @BranchCode, product_type = @ProductType, status = @Status WHERE id = @Id";
        await connection.ExecuteAsync(sql, account);
    }
}
