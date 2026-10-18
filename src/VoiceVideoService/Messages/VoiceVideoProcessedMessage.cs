namespace VoiceVideoService.Messages;

public class VoiceVideoProcessedMessage
{
    public Guid MessageId { get; set; }

    public Guid SessionId { get; set; }

    public DateTime ProcessedAt { get; set; }
}
