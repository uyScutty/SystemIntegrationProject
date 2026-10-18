namespace VoiceVideoService.Messages;

public class SubscriberReadiness
{
    private readonly TaskCompletionSource _ready =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Ready => _ready.Task;

    public void MarkReady()
    {
        _ready.TrySetResult();
    }
}