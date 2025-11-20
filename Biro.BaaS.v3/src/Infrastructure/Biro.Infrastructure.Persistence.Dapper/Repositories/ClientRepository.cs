using Biro.Core.Application.Repositories;
using Biro.Core.Domain.Entities;
using Dapper;

namespace Biro.Infrastructure.Persistence.Dapper.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ClientRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Client> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM clients WHERE id = @id";
        return await connection.QueryFirstOrDefaultAsync<Client>(sql, new { id });
    }

    public async Task AddAsync(Client client)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        const string sql = "INSERT INTO clients (id, name, document) VALUES (@Id, @Name, @Document)";
        await connection.ExecuteAsync(sql, client);
    }

    public async Task UpdateAsync(Client client)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        const string sql = "UPDATE clients SET name = @Name, document = @Document WHERE id = @Id";
        await connection.ExecuteAsync(sql, client);
    }
}
