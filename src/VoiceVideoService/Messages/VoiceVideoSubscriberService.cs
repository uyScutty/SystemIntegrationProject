
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;
using VoiceVideoService.Messages;

namespace VoiceVideoService.Messages;

public class VoiceVideoSubscriberService : BackgroundService
{
    private readonly IMessageClient _messageClient;
    private readonly ILogger<VoiceVideoSubscriberService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly SubscriberReadiness _readiness;

    public VoiceVideoSubscriberService(
        IMessageClient messageClient,
        ILogger<VoiceVideoSubscriberService> logger,
        IServiceScopeFactory scopeFactory,
        SubscriberReadiness readiness)
    {
        _messageClient = messageClient;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _readiness = readiness;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "VoiceVideo subscriber starter...");

        await _messageClient.SubscribeAsync<MessagePostedEvent>(
            subscriptionId: "voice-video-message-posted-sub",

            onMessage: async message =>
            {
                _logger.LogInformation(
                    "Modtog MessagePostedEvent: {MessageId}",
                    message.MessageId);

                using var scope = _scopeFactory.CreateScope();

                var handler = scope.ServiceProvider
                    .GetRequiredService<MessagePostedHandler>();

                await handler.Handle(message, stoppingToken);
            },

            cancellationToken: stoppingToken
        );

        _readiness.MarkReady();
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
