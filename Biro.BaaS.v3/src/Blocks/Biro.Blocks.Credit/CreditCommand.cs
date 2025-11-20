namespace Biro.Blocks.Credit;

public class CreditCommand
{
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string? WebhookUrl { get; set; }
}
