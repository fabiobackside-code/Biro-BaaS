using Domain.Core.Entities;
using Domain.Core.Ports.Outbound;
using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Adapters.Outbound.Persistence.SqlServer.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly IDbConnection _connection;
        private readonly Func<IDbTransaction> _transactionFactory;

        public ClientRepository(IDbConnection connection, Func<IDbTransaction> transactionFactory)
        {
            _connection = connection;
            _transactionFactory = transactionFactory;
        }

        public async Task CreateClientAsync(Client client)
        {
            var sql = "INSERT INTO Clients (ClientId, TaxId, FullName, Email, Phone, DateOfBirth, Status, CreatedAt, UpdatedAt) VALUES (@ClientId, @TaxId, @FullName, @Email, @Phone, @DateOfBirth, @Status, @CreatedAt, @UpdatedAt)";
            await _connection.ExecuteAsync(sql, client, _transactionFactory());
        }

        public async Task<Client> GetClientByIdAsync(Guid clientId)
        {
            var sql = "SELECT * FROM Clients WHERE ClientId = @ClientId";
            return await _connection.QuerySingleOrDefaultAsync<Client>(sql, new { ClientId = clientId }, _transactionFactory());
        }

        public async Task UpdateClientAsync(Client client)
        {
            var sql = "UPDATE Clients SET FullName = @FullName, Email = @Email, Phone = @Phone, Status = @Status, UpdatedAt = @UpdatedAt WHERE ClientId = @ClientId";
            await _connection.ExecuteAsync(sql, client, _transactionFactory());
        }
    }
}
