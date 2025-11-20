using bks.sdk.Processing.Contracts;

namespace Biro.Blocks.Credit;

public class CreditTransaction : ITransaction<CreditResponse>
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
}
