namespace VoiceVideoService.Messages;

public class VoiceVideoProcessedMessage
{
    public Guid SessionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}