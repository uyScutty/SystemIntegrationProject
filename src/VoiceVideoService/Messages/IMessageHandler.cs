namespace VoiceVideoService.Messages;

public interface IMessageHandler<T>
{
    Task Handle(T message, CancellationToken cancellationToken);
}