using Microsoft.EntityFrameworkCore;
using VoiceVideoService.Data;
using VoiceVideoService.Messages;
using VoiceVideoService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<ICallSessionService, CallSessionService>();
builder.Services.AddScoped<MessagePostedHandler>();
builder.Services.AddSingleton<SubscriberReadiness>();

builder.Services.AddDbContext<VoiceVideoDbContext>(options =>
{
    var connectionString = builder.Configuration
        .GetConnectionString("VoiceVideoDb");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Connection string 'VoiceVideoDb' is not configured.");
    }

    options.UseSqlServer(connectionString);
});

builder.Services.AddMessageClient(builder.Configuration);
builder.Services.AddHostedService<VoiceVideoSubscriberService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<VoiceVideoDbContext>();

    await dbContext.Database.EnsureCreatedAsync();
}

app.MapControllers();

app.Run();

public partial class Program;
