namespace Biro.Blocks.Credit;

public class CreditRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string? WebhookUrl { get; set; }
}
