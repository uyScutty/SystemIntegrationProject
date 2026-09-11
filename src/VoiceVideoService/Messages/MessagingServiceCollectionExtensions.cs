using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VoiceVideoService.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessageClient(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["RabbitMQ"] ?? "host=localhost";

        services.AddEasyNetQ(connectionString);
        services.AddSingleton<IMessageClient, RabbitMQMessageClient>();

        return services;
    }
}