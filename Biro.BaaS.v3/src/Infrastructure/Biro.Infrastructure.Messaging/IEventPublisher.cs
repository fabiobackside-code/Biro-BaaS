namespace Biro.Infrastructure.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(T message) where T : class;
}
