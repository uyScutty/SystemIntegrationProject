
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.Events;
using VoiceVideoService.Data;
using VoiceVideoService.Models;
using VoiceVideoService.Messages;
using Xunit;

namespace VoiceVideoService.Tests.Integration;

public class MessagePostedIntegrationTests
{
    [RabbitMqIntegrationFact]
    public async Task MessagePostedEvent_IsConsumed_AndResultEventPublished()
    {
        // ARRANGE
        using var factory = new VoiceVideoWebApplicationFactory();

        await factory.InitializeDatabaseAsync();

        var messageId = Guid.NewGuid();
        var channelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        // Opret en aktiv CallSession i testdatabasen
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<VoiceVideoDbContext>();

            context.CallSessions.Add(new CallSession
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
            });

            await context.SaveChangesAsync();
        }

        // Hent vores rigtige RabbitMQ-klient
        var client = factory.Services
            .GetRequiredService<IMessageClient>();

        // Her gemmer vi resultatet, når det modtages
        var resultReceived =
            new TaskCompletionSource<CallChatActivityRecordedEvent>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        // Abonner på vores result-event
        await client.SubscribeAsync<CallChatActivityRecordedEvent>(
            $"test-call-activity-{Guid.NewGuid()}",
            result =>
            {
                if (result.MessageId == messageId)
                    resultReceived.TrySetResult(result);

                return Task.CompletedTask;
            });

        // Vent på at VoiceVideoSubscriberService er klar
        var readiness = factory.Services
            .GetRequiredService<SubscriberReadiness>();

        await readiness.Ready.WaitAsync(
            TimeSpan.FromSeconds(15));

        // ACT: Send beskeden gennem rigtig RabbitMQ
        await client.PublishAsync(new MessagePostedEvent
        {
            MessageId = messageId,
            ChannelId = channelId,
            AuthorId = authorId,
            Content = "Hello from integration test",
            PostedAt = DateTime.UtcNow
        });

        // Vent på vores result-event
        var result = await resultReceived.Task.WaitAsync(
            TimeSpan.FromSeconds(15));

        // ASSERT
        Assert.Equal(messageId, result.MessageId);
        Assert.Equal(channelId, result.ChannelId);
    }
}

public sealed class RabbitMqIntegrationFactAttribute : FactAttribute
{
    public RabbitMqIntegrationFactAttribute()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("RUN_RABBITMQ_INTEGRATION_TESTS"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            Skip = "Set RUN_RABBITMQ_INTEGRATION_TESTS=true and start RabbitMQ to run this test.";
        }
    }
}
