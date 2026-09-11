namespace VoiceVideoService.Messaging;

public interface IMessageClient
{
    // Sender (producerer) en besked ud på køen
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default);

    // Lytter (subscriber) på en kø efter en bestemt beskedtype
    Task SubscribeAsync<T>(string subscriptionId, Action<T> onMessage, CancellationToken cancellationToken = default);
}