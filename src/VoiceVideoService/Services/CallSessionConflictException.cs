namespace VoiceVideoService.Services;

public class CallSessionConflictException : Exception
{
    public CallSessionConflictException(string message)
        : base(message)
    {
    }
}
