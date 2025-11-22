namespace Domain.Core.Entities
{
    public class Account
    {
        public Guid AccountId { get; set; }
        public Guid ClientId { get; set; }
        public string ProductType { get; set; }
        public string BranchCode { get; set; }
        public string AccountNumber { get; set; }
        public string Status { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
