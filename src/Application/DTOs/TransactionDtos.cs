using System;

namespace Application.DTOs
{
    public class DebitRequest
    {
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
    }

    public class CreditRequest
    {
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
    }

    public class TransferRequest
    {
        public Guid SourceAccountId { get; set; }
        public Guid DestinationAccountId { get; set; }
        public decimal Amount { get; set; }
    }

    public class BlockRequest
    {
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
    }

    public class UnblockRequest
    {
        public Guid TransactionId { get; set; }
    }

    public class ReservationRequest
    {
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
    }

    public class SettleRequest
    {
        public Guid TransactionId { get; set; }
    }

    public class TransactionResponse
    {
        public Guid TransactionId { get; set; }
        public Guid AccountId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; }
    }
}
