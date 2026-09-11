using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VoiceVideoService.Messages;

namespace VoiceVideoService.Messaging;

public class VoiceVideoSubscriberService : BackgroundService
{
    private readonly IMessageClient _messageClient;
    private readonly ILogger<VoiceVideoSubscriberService> _logger;

    public VoiceVideoSubscriberService(IMessageClient messageClient, ILogger<VoiceVideoSubscriberService> logger)
    {
        _messageClient = messageClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscriber starter og lytter efter beskeder...");

        // Vi lytter efter vores specifikke beskedtype (VoiceVideoProcessedMessage)
        await _messageClient.SubscribeAsync<VoiceVideoProcessedMessage>(
            subscriptionId: "voice-video-processor-sub",
            onMessage: message =>
            {
                // Her lander beskeden, når den bliver modtaget fra RabbitMQ!
                _logger.LogInformation("Modtog besked! ID: {Id}, Status: {Status}", message.SessionId, message.Status);
                
                // Her kan du tilføje din forretningslogik (f.eks. gemme i database, starte AI-behandling, osv.)
            },
            cancellationToken: stoppingToken
        );

        // Vi sørger for, at baggrundstjenesten ikke lukker ned med det samme
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}