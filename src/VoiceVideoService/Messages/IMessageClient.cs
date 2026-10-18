namespace VoiceVideoService.Messages;

public interface IMessageClient
{
    // Sender (producerer) en besked ud på køen
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default);

    // Lytter (subscriber) på en kø efter en bestemt beskedtype
    Task SubscribeAsync<T>(string subscriptionId, Func<T, Task> onMessage, CancellationToken cancellationToken = default);
}