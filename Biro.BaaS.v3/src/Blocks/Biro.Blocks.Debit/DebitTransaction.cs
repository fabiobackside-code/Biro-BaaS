using bks.sdk.Processing.Contracts;

namespace Biro.Blocks.Debit;

public class DebitTransaction : ITransaction<DebitResponse>
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
}
