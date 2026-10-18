namespace SharedModels;

public class CallSessionDto
{
    public Guid CallSessionId { get; set; }

    public Guid ChannelId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string CallStatus { get; set; } = string.Empty;

    public DateTime? LastMessageAt { get; set; }

    public int ParticipantCount { get; set; }

    public List<CallParticipantDto> Participants { get; set; } = new();
}
