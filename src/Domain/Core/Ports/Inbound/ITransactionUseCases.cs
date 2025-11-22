using System;
using System.Threading.Tasks;

namespace Domain.Core.Ports.Inbound
{
    public interface ITransactionUseCases
    {
        Task DebitAsync(Guid accountId, decimal amount);
        Task CreditAsync(Guid accountId, decimal amount);
        Task TransferAsync(Guid sourceAccountId, Guid destinationAccountId, decimal amount);
        Task BlockAsync(Guid accountId, decimal amount);
        Task UnblockAsync(Guid transactionId);
        Task ReservationAsync(Guid accountId, decimal amount);
        Task SettleAsync(Guid transactionId);
    }
}
