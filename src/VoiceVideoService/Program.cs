using VoiceVideoService.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Tilføj controllers
builder.Services.AddControllers();

// Her kalder vi vores nye extension-metode, som henter config og sender den ind i klienten!
builder.Services.AddMessageClient(builder.Configuration);

// Registrer vores baggrundstjeneste/subscriber
builder.Services.AddHostedService<VoiceVideoSubscriberService>();

var app = builder.Build();

app.MapControllers();

app.Run();