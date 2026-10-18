using EasyNetQ;
using VoiceVideoService.Messages;

namespace VoiceVideoService.Messages;

public class RabbitMQMessageClient : IMessageClient, IAsyncDisposable
{
    private readonly IBus _bus;

    public RabbitMQMessageClient(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        await _bus.PubSub.PublishAsync(message, cancellationToken);
    }

    public async Task SubscribeAsync<T>(string subscriptionId, Func<T, Task> onMessage, CancellationToken cancellationToken = default)
    {
        await _bus.PubSub.SubscribeAsync(subscriptionId, onMessage, cancellationToken);
    }


    // Rettet til DisposeAsync da nyere EasyNetQ bruger IAsyncDisposable
    public async ValueTask DisposeAsync()
    {
        if (_bus is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
    }
}