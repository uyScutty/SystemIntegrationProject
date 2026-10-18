namespace Shared.Contracts.Events;

public class CallChatActivityRecordedEvent
{
    public Guid MessageId { get; set; }

    public Guid CallSessionId { get; set; }

    public Guid ChannelId { get; set; }

    public DateTime ProcessedAt { get; set; }
}