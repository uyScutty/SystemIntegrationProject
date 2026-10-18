
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;
using VoiceVideoService.Data;
using VoiceVideoService.Messages;
using VoiceVideoService.Models;
using VoiceVideoService.Tests.Fakes;
using Xunit;

namespace VoiceVideoService.Tests;

public class MessagePostedHandlerTests
{
    [Fact]
    public async Task Handle_PublishesResultEvent_WithCorrectMessageId()
    {
        // ARRANGE
        var messageId = Guid.NewGuid();
        var channelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<VoiceVideoDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new VoiceVideoDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var session = new CallSession
        {
            Id = Guid.NewGuid(),
            ChannelId = channelId,
            CallStatus = "Active",
            StartTime = DateTime.UtcNow,
            Participants = new List<CallParticipant>
            {
                new CallParticipant
                {
                    Id = Guid.NewGuid(),
                    UserId = authorId,
                    JoinedAt = DateTime.UtcNow
                }
            }
        };

        context.CallSessions.Add(session);
        await context.SaveChangesAsync();

        var client = new FakeMessageClient();
        var handler = new MessagePostedHandler(context, client);

        var message = new MessagePostedEvent
        {
            MessageId = messageId,
            ChannelId = channelId,
            AuthorId = authorId,
            Content = "Hello world",
            PostedAt = DateTime.UtcNow
        };

        // ACT
        await handler.Handle(message, CancellationToken.None);

        // ASSERT
        var published = client.SinglePublished<CallChatActivityRecordedEvent>();

        Assert.NotNull(published);
        Assert.Equal(messageId, published.MessageId);
        Assert.Equal(session.Id, published.CallSessionId);
        Assert.Equal(message.PostedAt, session.LastMessageAt);
    }


[Fact]
public async Task Handler_CanConsume_MinimumValidContract()
{
    // ARRANGE
    var client = new FakeMessageClient();

    using var connection = new SqliteConnection("DataSource=:memory:");
    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<VoiceVideoDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context = new VoiceVideoDbContext(options);
    await context.Database.EnsureCreatedAsync();

    var message = new MessagePostedEvent
    {
        MessageId = Guid.NewGuid(),
        ChannelId = Guid.NewGuid(),
        AuthorId = Guid.NewGuid(),
        Content = "Hello world",
        PostedAt = DateTime.UtcNow
    };

    var handler = new MessagePostedHandler(context, client);

    // ACT
    var exception = await Record.ExceptionAsync(
        () => handler.Handle(message, CancellationToken.None));

    // ASSERT
    Assert.Null(exception);
    Assert.Empty(client.Published);
}

    [Fact]
    public async Task Handle_WhenMessageIsDeliveredTwice_PublishesOnlyOnce()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<VoiceVideoDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new VoiceVideoDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var authorId = Guid.NewGuid();
        var channelId = Guid.NewGuid();
        context.CallSessions.Add(new CallSession
        {
            Id = Guid.NewGuid(),
            ChannelId = channelId,
            StartTime = DateTime.UtcNow,
            Participants =
            [
                new CallParticipant
                {
                    Id = Guid.NewGuid(),
                    UserId = authorId,
                    JoinedAt = DateTime.UtcNow
                }
            ]
        });
        await context.SaveChangesAsync();

        var client = new FakeMessageClient();
        var handler = new MessagePostedHandler(context, client);
        var message = new MessagePostedEvent
        {
            MessageId = Guid.NewGuid(),
            ChannelId = channelId,
            AuthorId = authorId,
            Content = "Duplicate delivery",
            PostedAt = DateTime.UtcNow
        };

        await handler.Handle(message, CancellationToken.None);
        await handler.Handle(message, CancellationToken.None);

        Assert.Single(client.Published);
        Assert.Equal(1, await context.ProcessedMessages.CountAsync());
    }

}
