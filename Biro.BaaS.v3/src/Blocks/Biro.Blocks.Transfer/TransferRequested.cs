namespace Biro.Blocks.Transfer;

public class TransferRequested
{
    public Guid TransferId { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
}
