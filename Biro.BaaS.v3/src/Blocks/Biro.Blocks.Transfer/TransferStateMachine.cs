using MassTransit;

namespace Biro.Blocks.Transfer;

public class TransferStateMachine : MassTransitStateMachine<TransferState>
{
    public State DebitCompleted { get; private set; }
    public State CreditCompleted { get; private set; }
    public State TransferFailed { get; private set; }

    public Event<TransferRequested> TransferRequested { get; private set; }
    public Event<DebitCompleted> DebitCompletedEvent { get; private set; }
    public Event<CreditCompleted> CreditCompletedEvent { get; private set; }

    public TransferStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => TransferRequested, x => x.CorrelateById(context => context.Message.TransferId));
        Event(() => DebitCompletedEvent, x => x.CorrelateById(context => context.Message.TransferId));
        Event(() => CreditCompletedEvent, x => x.CorrelateById(context => context.Message.TransferId));

        Initially(
            When(TransferRequested)
                .Then(context =>
                {
                    context.Saga.FromAccountId = context.Message.FromAccountId;
                    context.Saga.ToAccountId = context.Message.ToAccountId;
                    context.Saga.Amount = context.Message.Amount;
                })
                .Publish(context => new DebitCommand { TransferId = context.Saga.CorrelationId, AccountId = context.Saga.FromAccountId, Amount = context.Saga.Amount })
                .TransitionTo(DebitCompleted)
        );

        During(DebitCompleted,
            When(DebitCompletedEvent)
                .Publish(context => new CreditCommand { TransferId = context.Saga.CorrelationId, AccountId = context.Saga.ToAccountId, Amount = context.Saga.Amount })
                .TransitionTo(CreditCompleted)
        );

        During(CreditCompleted,
            When(CreditCompletedEvent)
                .Finalize()
        );
    }
}
