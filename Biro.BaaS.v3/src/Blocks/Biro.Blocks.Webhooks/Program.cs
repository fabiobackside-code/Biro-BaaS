using Biro.Blocks.Webhooks;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<WebhookNotificationConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.Run();
