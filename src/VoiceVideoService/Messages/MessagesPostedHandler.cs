using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;
using VoiceVideoService.Data;

namespace VoiceVideoService.Messages;

public class MessagePostedHandler : IMessageHandler<MessagePostedEvent>
{
    private readonly VoiceVideoDbContext _context;
    private readonly IMessageClient _messageClient;

    public MessagePostedHandler(
        VoiceVideoDbContext context,
        IMessageClient messageClient)
    {
        _context = context;
        _messageClient = messageClient;
    }

    public async Task Handle(
        MessagePostedEvent message,
        CancellationToken cancellationToken)
    {
        var alreadyProcessed = await _context.ProcessedMessages
            .AnyAsync(
                processed => processed.MessageId == message.MessageId,
                cancellationToken);

        if (alreadyProcessed)
        {
            return;
        }

        var session = await _context.CallSessions
            .Include(callSession => callSession.Participants)
            .FirstOrDefaultAsync(callSession =>
                callSession.ChannelId == message.ChannelId &&
                callSession.CallStatus == "Active" &&
                callSession.Participants.Any(participant =>
                    participant.UserId == message.AuthorId &&
                    participant.LeftAt == null),
                cancellationToken);

        if (session is null)
        {
            return;
        }

        if (session.LastMessageAt is null || message.PostedAt > session.LastMessageAt)
        {
            session.LastMessageAt = message.PostedAt;
        }

        var processedAt = DateTime.UtcNow;
        _context.ProcessedMessages.Add(new VoiceVideoProcessedMessage
        {
            MessageId = message.MessageId,
            SessionId = session.Id,
            ProcessedAt = processedAt
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _messageClient.PublishAsync(
            new CallChatActivityRecordedEvent
            {
                MessageId = message.MessageId,
                CallSessionId = session.Id,
                ChannelId = session.ChannelId,
                ProcessedAt = processedAt
            },
            cancellationToken);
    }
}
