using Domain.Core.Ports.Outbound;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace Adapters.Outbound.Persistence.SqlServer
{
    public class UnitOfWork : IUnitOfWork
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private readonly string _connectionString;
        private bool _disposed;

        public IClientRepository Clients { get; }
        public IAccountRepository Accounts { get; }
        public ITransactionRepository Transactions { get; }

        public UnitOfWork(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);

            Clients = new Repositories.ClientRepository(_connection, () => _transaction);
            Accounts = new Repositories.AccountRepository(_connection, () => _transaction);
            Transactions = new Repositories.TransactionRepository(_connection, () => _transaction);
        }

        public async Task BeginTransactionAsync()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                await (_connection as SqlConnection).OpenAsync();
            }
            _transaction = _connection.BeginTransaction();
        }

        public async Task CommitAsync()
        {
            try
            {
                await (_transaction as SqlTransaction).CommitAsync();
            }
            catch
            {
                await (_transaction as SqlTransaction).RollbackAsync();
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await (_transaction as SqlTransaction).RollbackAsync();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _connection?.Dispose();
                }
            }
            _disposed = true;
        }
    }
}
