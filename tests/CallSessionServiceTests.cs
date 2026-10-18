using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VoiceVideoService.Data;
using VoiceVideoService.Models;
using VoiceVideoService.Services;

namespace VoiceVideoService.Tests;

public class CallSessionServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesActiveSession()
    {
        await using var connection = await OpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = new CallSessionService(context);
        var channelId = Guid.NewGuid();

        var session = await service.CreateAsync(channelId);

        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.Equal(channelId, session.ChannelId);
        Assert.Equal("Active", session.CallStatus);
        Assert.Null(session.EndTime);
        Assert.Equal(1, await context.CallSessions.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_WhenChannelAlreadyHasActiveSession_ThrowsConflict()
    {
        await using var connection = await OpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = new CallSessionService(context);
        var channelId = Guid.NewGuid();
        await service.CreateAsync(channelId);

        await Assert.ThrowsAsync<CallSessionConflictException>(
            () => service.CreateAsync(channelId));
    }

    [Fact]
    public async Task GetAllAsync_LoadsParticipants()
    {
        await using var connection = await OpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var session = new CallSession
        {
            Id = Guid.NewGuid(),
            ChannelId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            Participants =
            [
                new CallParticipant
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    JoinedAt = DateTime.UtcNow
                }
            ]
        };
        context.CallSessions.Add(session);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var result = await new CallSessionService(context).GetAllAsync();

        Assert.Single(Assert.Single(result).Participants);
    }

    [Fact]
    public async Task EndAndDeleteAsync_ChangeSessionLifecycle()
    {
        await using var connection = await OpenConnectionAsync();
        await using var context = await CreateContextAsync(connection);
        var service = new CallSessionService(context);
        var session = await service.CreateAsync(Guid.NewGuid());

        Assert.True(await service.EndSessionAsync(session.Id));
        context.ChangeTracker.Clear();
        var ended = await context.CallSessions.SingleAsync();
        Assert.Equal("Ended", ended.CallStatus);
        Assert.NotNull(ended.EndTime);

        Assert.True(await service.DeleteAsync(session.Id));
        Assert.Empty(await context.CallSessions.ToListAsync());
    }

    private static async Task<SqliteConnection> OpenConnectionAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        return connection;
    }

    private static async Task<VoiceVideoDbContext> CreateContextAsync(
        SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<VoiceVideoDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new VoiceVideoDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }
}
