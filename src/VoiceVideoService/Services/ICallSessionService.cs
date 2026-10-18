namespace VoiceVideoService.Services;

using VoiceVideoService.Models;

public interface ICallSessionService
{
    Task<CallSession> CreateAsync(
        Guid channelId,
        CancellationToken cancellationToken = default);

    Task<CallSession?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<CallSession>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<bool> EndSessionAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
