using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SharedModels;
using VoiceVideoService.Data;
using VoiceVideoService.Models;

namespace VoiceVideoService.Tests.Integration;

public class CallSessionApiTests
{
    [Fact]
    public async Task PostThenGet_ReturnsCreatedSession()
    {
        using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateClient();
        var channelId = Guid.NewGuid();

        var createResponse = await client.PostAsJsonAsync(
            "/api/voicevideo/sessions",
            new { ChannelId = channelId });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CallSessionDto>();
        Assert.NotNull(created);
        Assert.Equal(channelId, created.ChannelId);
        Assert.Equal("Active", created.CallStatus);
        Assert.NotNull(createResponse.Headers.Location);

        var returned = await client.GetFromJsonAsync<CallSessionDto>(
            createResponse.Headers.Location);

        Assert.NotNull(returned);
        Assert.Equal(created.CallSessionId, returned.CallSessionId);
    }

    [Fact]
    public async Task Post_WithEmptyChannelId_ReturnsBadRequest()
    {
        using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/voicevideo/sessions",
            new { ChannelId = Guid.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsParticipantInformation()
    {
        using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateClient();
        var userId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<VoiceVideoDbContext>();
            context.CallSessions.Add(new CallSession
            {
                Id = Guid.NewGuid(),
                ChannelId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow,
                Participants =
                [
                    new CallParticipant
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        JoinedAt = DateTime.UtcNow
                    }
                ]
            });
            await context.SaveChangesAsync();
        }

        var sessions = await client.GetFromJsonAsync<List<CallSessionDto>>(
            "/api/voicevideo/sessions");

        var session = Assert.Single(sessions!);
        Assert.Equal(1, session.ParticipantCount);
        Assert.Equal(userId.ToString(), Assert.Single(session.Participants).UserId);
    }

    [Fact]
    public async Task PutEndThenDelete_UpdatesAndRemovesSession()
    {
        using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/voicevideo/sessions",
            new { ChannelId = Guid.NewGuid() });
        var created = await createResponse.Content.ReadFromJsonAsync<CallSessionDto>();

        var endResponse = await client.PutAsync(
            $"/api/voicevideo/sessions/{created!.CallSessionId}/end",
            content: null);
        Assert.Equal(HttpStatusCode.NoContent, endResponse.StatusCode);

        var ended = await client.GetFromJsonAsync<CallSessionDto>(
            $"/api/voicevideo/sessions/{created.CallSessionId}");
        Assert.Equal("Ended", ended!.CallStatus);
        Assert.NotNull(ended.EndTime);

        var deleteResponse = await client.DeleteAsync(
            $"/api/voicevideo/sessions/{created.CallSessionId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingResponse = await client.GetAsync(
            $"/api/voicevideo/sessions/{created.CallSessionId}");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }
}
