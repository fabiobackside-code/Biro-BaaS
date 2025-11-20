namespace Biro.Shared.Contracts;

public class TransactionCompleted
{
    public Guid TransactionId { get; set; }
    public string Status { get; set; }
    public decimal Amount { get; set; }
    public Guid AccountId { get; set; }
    public string WebhookUrl { get; set; }
}
