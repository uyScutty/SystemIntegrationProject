using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace VoiceVideoService.Messages;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessageClient(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["RabbitMQ"] ?? "HOST=localhost";

        services.AddEasyNetQ(connectionString);
        services.AddSingleton<IMessageClient, RabbitMQMessageClient>();

        return services;
    }
}