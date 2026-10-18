namespace VoiceVideoService.Services;

using Microsoft.EntityFrameworkCore;
using VoiceVideoService.Data;
using VoiceVideoService.Models;

public class CallSessionService : ICallSessionService
{
    private readonly VoiceVideoDbContext _dbContext;

    public CallSessionService(VoiceVideoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CallSession> CreateAsync(
        Guid channelId,
        CancellationToken cancellationToken = default)
    {
        if (channelId == Guid.Empty)
        {
            throw new ArgumentException("ChannelId cannot be empty.", nameof(channelId));
        }

        var activeSessionExists = await _dbContext.CallSessions
            .AnyAsync(
                session => session.ChannelId == channelId && session.CallStatus == "Active",
                cancellationToken);

        if (activeSessionExists)
        {
            throw new CallSessionConflictException(
                "An active call session already exists for this channel.");
        }

        var session = new CallSession
        {
            Id = Guid.NewGuid(),
            ChannelId = channelId,
            StartTime = DateTime.UtcNow,
            CallStatus = "Active"
        };

        _dbContext.CallSessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return session;
    }

    public Task<CallSession?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallSessions
            .AsNoTracking()
            .Include(session => session.Participants)
            .FirstOrDefaultAsync(session => session.Id == id, cancellationToken);
    }

    public Task<List<CallSession>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallSessions
            .AsNoTracking()
            .Include(session => session.Participants)
            .OrderByDescending(session => session.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EndSessionAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.CallSessions
            .FirstOrDefaultAsync(session => session.Id == id, cancellationToken);

        if (session is null || session.CallStatus == "Ended")
        {
            return false;
        }

        session.CallStatus = "Ended";
        session.EndTime = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.CallSessions
            .FirstOrDefaultAsync(session => session.Id == id, cancellationToken);

        if (session is null)
        {
            return false;
        }

        _dbContext.CallSessions.Remove(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
