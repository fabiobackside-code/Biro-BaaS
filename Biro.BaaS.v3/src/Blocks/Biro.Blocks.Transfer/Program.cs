using Biro.Blocks.Transfer;
using Biro.Infrastructure.Messaging;
using bks.sdk.Core.Initialization;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBKSFramework(builder.Configuration);

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddSagaStateMachine<TransferStateMachine, TransferState>()
        .InMemoryRepository();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
        cfg.ConfigureEndpoints(context);
    });
});


var app = builder.Build();

app.UseBKSFramework();

app.MapPost("/transfer", async ([FromBody] TransferRequest request, IBus bus) =>
{
    var transferId = NewId.NextGuid();
    await bus.Publish(new TransferRequested
    {
        TransferId = transferId,
        FromAccountId = request.FromAccountId,
        ToAccountId = request.ToAccountId,
        Amount = request.Amount
    });
    return Results.Accepted(value: new { TransferId = transferId });
});

app.Run();
