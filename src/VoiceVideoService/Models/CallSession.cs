namespace VoiceVideoService.Models;

public class CallSession
{
    public Guid Id { get; set; }

    public Guid ChannelId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string CallStatus { get; set; } = "Active";

    public List<CallParticipant> Participants { get; set; } = new();

    public DateTime? LastMessageAt { get; set; }
}
