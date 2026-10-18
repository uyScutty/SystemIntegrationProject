namespace SharedModels;

public class CallParticipantDto
{
    public string UserId { get; set; } = string.Empty;

    public DateTime JoinedAt { get; set; }

    public DateTime? LeftAt { get; set; }

    public bool IsMuted { get; set; }

    public bool IsVideoEnabled { get; set; }

    public bool IsScreenSharing { get; set; }
}
