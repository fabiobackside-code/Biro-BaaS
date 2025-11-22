using System;
using System.Threading.Tasks;

namespace Domain.Core.Ports.Outbound
{
    public interface IUnitOfWork : IDisposable
    {
        IClientRepository Clients { get; }
        IAccountRepository Accounts { get; }
        ITransactionRepository Transactions { get; }

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
