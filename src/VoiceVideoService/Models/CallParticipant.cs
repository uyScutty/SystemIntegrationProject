namespace VoiceVideoService.Models;

public class CallParticipant
{
    public Guid Id { get; set; }

    public Guid CallSessionId { get; set; }

    public CallSession? CallSession { get; set; }

    public Guid UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public DateTime? LeftAt { get; set; }

    public bool IsMuted { get; set; }

    public bool IsVideoEnabled { get; set; }

    public bool IsScreenSharing { get; set; }
}
