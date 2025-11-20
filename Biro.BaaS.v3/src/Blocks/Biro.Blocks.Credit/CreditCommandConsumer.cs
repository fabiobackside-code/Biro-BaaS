using Biro.Shared.Contracts;
using bks.sdk.Processing.Contracts;
using MassTransit;

namespace Biro.Blocks.Credit;

public class CreditCommandConsumer : IConsumer<CreditCommand>
{
    private readonly IPipelineExecutor _pipelineExecutor;
    private readonly IBus _bus;

    public CreditCommandConsumer(IPipelineExecutor pipelineExecutor, IBus bus)
    {
        _pipelineExecutor = pipelineExecutor;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<CreditCommand> context)
    {
        var transaction = new CreditTransaction
        {
            AccountId = context.Message.AccountId,
            Amount = context.Message.Amount
        };
        var result = await _pipelineExecutor.ExecuteAsync<CreditTransaction, CreditResponse>(transaction);

        await _bus.Publish(new TransactionCompleted
        {
            TransactionId = context.Message.TransactionId,
            Status = result.IsSuccess ? "Completed" : "Failed",
            Amount = context.Message.Amount,
            AccountId = context.Message.AccountId,
            WebhookUrl = context.Message.WebhookUrl
        });
    }
}
