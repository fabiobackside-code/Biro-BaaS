using System;

namespace Application.DTOs
{
    public class CreateAccountRequest
    {
        public Guid ClientId { get; set; }
        public string ProductType { get; set; }
        public string BranchCode { get; set; }
        public string AccountNumber { get; set; }
    }

    public class AccountDetailsResponse
    {
        public Guid AccountId { get; set; }
        public Guid ClientId { get; set; }
        public string ProductType { get; set; }
        public string BranchCode { get; set; }
        public string AccountNumber { get; set; }
        public string Status { get; set; }
        public DateTime OpenedAt { get; set; }
    }
}
