using Biro.Shared.Contracts;
using bks.sdk.Processing.Contracts;
using MassTransit;

namespace Biro.Blocks.Debit;

public class DebitCommandConsumer : IConsumer<DebitCommand>
{
    private readonly IPipelineExecutor _pipelineExecutor;
    private readonly IBus _bus;

    public DebitCommandConsumer(IPipelineExecutor pipelineExecutor, IBus bus)
    {
        _pipelineExecutor = pipelineExecutor;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<DebitCommand> context)
    {
        var transaction = new DebitTransaction
        {
            AccountId = context.Message.AccountId,
            Amount = context.Message.Amount
        };
        var result = await _pipelineExecutor.ExecuteAsync<DebitTransaction, DebitResponse>(transaction);

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
