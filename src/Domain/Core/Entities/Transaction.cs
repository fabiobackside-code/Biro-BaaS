namespace Domain.Core.Entities
{
    public class Transaction
    {
        public Guid TransactionId { get; set; }
        public Guid AccountId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid CorrelationId { get; set; }
        public string Status { get; set; }
        public DateTime? ExpirationDateTime { get; set; }
        public string Metadata { get; set; }
    }
}
