using Domain.Core.Entities;
using Domain.Core.Ports.Inbound;
using Domain.Core.Ports.Outbound;
using System;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class TransactionUseCases : ITransactionUseCases
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionUseCases(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreditAsync(Guid accountId, decimal amount)
        {
            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                AccountId = accountId,
                TransactionType = "CREDIT",
                Amount = amount,
                Timestamp = DateTime.UtcNow,
                CorrelationId = Guid.NewGuid(),
                Status = "COMPLETED"
            };
            await _unitOfWork.Transactions.CreateTransactionAsync(transaction);
        }

        public async Task DebitAsync(Guid accountId, decimal amount)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var balance = await _unitOfWork.Accounts.GetAccountBalanceAsync(accountId);
                if (balance < amount)
                {
                    throw new InvalidOperationException("Insufficient funds.");
                }

                var debitTransaction = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    AccountId = accountId,
                    TransactionType = "DEBIT",
                    Amount = amount,
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = Guid.NewGuid(),
                    Status = "COMPLETED"
                };
                await _unitOfWork.Transactions.CreateTransactionAsync(debitTransaction);

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task TransferAsync(Guid sourceAccountId, Guid destinationAccountId, decimal amount)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var balance = await _unitOfWork.Accounts.GetAccountBalanceAsync(sourceAccountId);
                if (balance < amount)
                {
                    throw new InvalidOperationException("Insufficient funds.");
                }

                var debitTransaction = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    AccountId = sourceAccountId,
                    TransactionType = "DEBIT",
                    Amount = amount,
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = Guid.NewGuid(),
                    Status = "COMPLETED"
                };
                await _unitOfWork.Transactions.CreateTransactionAsync(debitTransaction);

                var creditTransaction = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    AccountId = destinationAccountId,
                    TransactionType = "CREDIT",
                    Amount = amount,
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = Guid.NewGuid(),
                    Status = "COMPLETED"
                };
                await _unitOfWork.Transactions.CreateTransactionAsync(creditTransaction);

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task BlockAsync(Guid accountId, decimal amount)
        {
            var balance = await _unitOfWork.Accounts.GetAccountBalanceAsync(accountId);
            if (balance < amount)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                AccountId = accountId,
                TransactionType = "BLOCK",
                Amount = amount,
                Timestamp = DateTime.UtcNow,
                CorrelationId = Guid.NewGuid(),
                Status = "ACTIVE"
            };
            await _unitOfWork.Transactions.CreateTransactionAsync(transaction);
        }

        public async Task UnblockAsync(Guid transactionId)
        {
            var transaction = await _unitOfWork.Transactions.GetTransactionByIdAsync(transactionId);
            if (transaction == null || transaction.TransactionType != "BLOCK")
            {
                throw new InvalidOperationException("Invalid transaction for unblock operation.");
            }

            transaction.Status = "CANCELLED";
            await _unitOfWork.Transactions.UpdateTransactionAsync(transaction);
        }

        public async Task ReservationAsync(Guid accountId, decimal amount)
        {
            var balance = await _unitOfWork.Accounts.GetAccountBalanceAsync(accountId);
            if (balance < amount)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                AccountId = accountId,
                TransactionType = "RESERVATION",
                Amount = amount,
                Timestamp = DateTime.UtcNow,
                CorrelationId = Guid.NewGuid(),
                Status = "ACTIVE"
            };
            await _unitOfWork.Transactions.CreateTransactionAsync(transaction);
        }

        public async Task SettleAsync(Guid transactionId)
        {
            var transaction = await _unitOfWork.Transactions.GetTransactionByIdAsync(transactionId);
            if (transaction == null || (transaction.TransactionType != "BLOCK" && transaction.TransactionType != "RESERVATION"))
            {
                throw new InvalidOperationException("Invalid transaction for settle operation.");
            }

            transaction.Status = "SETTLED";
            await _unitOfWork.Transactions.UpdateTransactionAsync(transaction);
        }
    }
}
