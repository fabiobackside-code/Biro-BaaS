namespace Biro.Blocks.Transfer;

public class DebitCommand
{
    public Guid TransferId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
}
