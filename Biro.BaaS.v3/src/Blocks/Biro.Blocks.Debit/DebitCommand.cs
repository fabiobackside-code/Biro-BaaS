namespace Biro.Blocks.Debit;

public class DebitCommand
{
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string? WebhookUrl { get; set; }
}
