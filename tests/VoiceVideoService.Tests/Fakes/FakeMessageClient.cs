using VoiceVideoService.Messages;

namespace VoiceVideoService.Tests.Fakes;

public class FakeMessageClient : IMessageClient
{
    public List<object> Published { get; } = new();

    public Task PublishAsync<T>(
        T message,
        CancellationToken cancellationToken = default)
    {
        Published.Add(message!);
        return Task.CompletedTask;
    }

    public Task SubscribeAsync<T>(
        string subscriptionId,
        Func<T, Task> onMessage,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public T? SinglePublished<T>() where T : class
    {
        return Published.OfType<T>().SingleOrDefault();
    }
}