namespace Biro.Blocks.Debit;

public class DebitRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string? WebhookUrl { get; set; }
}
