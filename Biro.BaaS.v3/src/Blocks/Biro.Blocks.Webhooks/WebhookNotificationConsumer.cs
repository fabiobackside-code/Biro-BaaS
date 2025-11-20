using Biro.Shared.Contracts;
using MassTransit;

namespace Biro.Blocks.Webhooks;

public class WebhookNotificationConsumer : IConsumer<TransactionCompleted>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookNotificationConsumer> _logger;

    public WebhookNotificationConsumer(IHttpClientFactory httpClientFactory, ILogger<WebhookNotificationConsumer> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TransactionCompleted> context)
    {
        var httpClient = _httpClientFactory.CreateClient();
        try
        {
            await httpClient.PostAsJsonAsync(context.Message.WebhookUrl, context.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook for transaction {TransactionId}", context.Message.TransactionId);
        }
    }
}
