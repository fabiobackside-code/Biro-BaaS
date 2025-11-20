using Biro.Blocks.Credit;
using Biro.Core.Application.Repositories;
using Biro.Infrastructure.Cache;
using Biro.Infrastructure.Messaging;
using Biro.Infrastructure.Persistence.Dapper;
using bks.sdk.Core.Initialization;
using bks.sdk.Processing.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBKSFramework(builder.Configuration);
builder.Services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddScoped<ICacheProvider, RedisCacheProvider>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CreditCommandConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseMiddleware<IdempotencyMiddleware>();

app.UseBKSFramework();

app.MapPost("/credit", async ([FromBody] CreditRequest request, IBus bus) =>
{
    var transactionId = NewId.NextGuid();
    await bus.Publish(new CreditCommand { TransactionId = transactionId, AccountId = request.AccountId, Amount = request.Amount });
    return Results.Accepted(value: new { TransactionId = transactionId });
});

app.MapGet("/status/{transactionId}", (Guid transactionId) =>
{
    // In a real application, you would check the status of the transaction in a database.
    // For this example, we'll just return a placeholder status.
    return Results.Ok(new { TransactionId = transactionId, Status = "Pending" });
});

app.Run();
