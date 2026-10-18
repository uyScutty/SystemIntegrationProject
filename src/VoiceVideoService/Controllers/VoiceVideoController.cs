using Microsoft.AspNetCore.Mvc;
using SharedModels;
using VoiceVideoService.Contracts;
using VoiceVideoService.Models;
using VoiceVideoService.Services;

namespace VoiceVideoService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VoiceVideoController : ControllerBase
{
    private readonly ICallSessionService _callSessionService;

    public VoiceVideoController(ICallSessionService callSessionService)
    {
        _callSessionService = callSessionService;
    }

    [HttpGet]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            Message = "Microservice is up and running!",
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpPost("sessions")]
    public async Task<ActionResult<CallSessionDto>> CreateSession(
        [FromBody] CreateCallSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ChannelId == Guid.Empty)
        {
            ModelState.AddModelError(nameof(request.ChannelId), "ChannelId is required.");
            return ValidationProblem(ModelState);
        }

        try
        {
            var session = await _callSessionService.CreateAsync(
                request.ChannelId,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetSessionById),
                new { id = session.Id },
                ToDto(session));
        }
        catch (CallSessionConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Call session conflict",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<IReadOnlyCollection<CallSessionDto>>> GetAllSessions(
        CancellationToken cancellationToken)
    {
        var sessions = await _callSessionService.GetAllAsync(cancellationToken);
        return Ok(sessions.Select(ToDto).ToList());
    }

    [HttpGet("sessions/{id:guid}")]
    public async Task<ActionResult<CallSessionDto>> GetSessionById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var session = await _callSessionService.GetByIdAsync(id, cancellationToken);

        if (session is null)
        {
            return NotFound();
        }

        return Ok(ToDto(session));
    }

    [HttpPut("sessions/{id:guid}/end")]
    public async Task<IActionResult> EndSession(
        Guid id,
        CancellationToken cancellationToken)
    {
        var session = await _callSessionService.GetByIdAsync(id, cancellationToken);

        if (session is null)
        {
            return NotFound();
        }

        if (session.CallStatus == "Ended")
        {
            return Conflict("Session is already ended.");
        }

        var success = await _callSessionService.EndSessionAsync(id, cancellationToken);
        return success
            ? NoContent()
            : Conflict("Session could not be ended.");
    }

    [HttpDelete("sessions/{id:guid}")]
    public async Task<IActionResult> DeleteSession(
        Guid id,
        CancellationToken cancellationToken)
    {
        var success = await _callSessionService.DeleteAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    private static CallSessionDto ToDto(CallSession session)
    {
        return new CallSessionDto
        {
            CallSessionId = session.Id,
            ChannelId = session.ChannelId,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            CallStatus = session.CallStatus,
            LastMessageAt = session.LastMessageAt,
            ParticipantCount = session.Participants.Count,
            Participants = session.Participants.Select(participant =>
                new CallParticipantDto
                {
                    UserId = participant.UserId.ToString(),
                    JoinedAt = participant.JoinedAt,
                    LeftAt = participant.LeftAt,
                    IsMuted = participant.IsMuted,
                    IsVideoEnabled = participant.IsVideoEnabled,
                    IsScreenSharing = participant.IsScreenSharing
                }).ToList()
        };
    }
}
