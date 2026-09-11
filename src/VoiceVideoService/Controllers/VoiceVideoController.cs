using Microsoft.AspNetCore.Mvc;
using VoiceVideoService.Messages;
using VoiceVideoService.Messaging;

namespace VoiceVideoService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VoiceVideoController : ControllerBase
{
    private readonly IMessageClient _messageClient;

    // Her bruger vi Constructor Injection! 
    // .NET opdager automatisk at vi mangler IMessageClient og sender den ind.
    public VoiceVideoController(IMessageClient messageClient)
    {
        _messageClient = messageClient;
    }

    [HttpGet]
    public IActionResult GetStatus()
    {
        return Ok(new { Message = "Microservice is up and running!", Timestamp = DateTime.UtcNow });
    }

    // Et nyt endpoint til at teste afsendelse af en besked (Publish)
    [HttpPost("publish")]
    public async Task<IActionResult> PublishMessage([FromBody] string text)
    {
        // 1. Vi opretter vores DTO-besked
        var message = new VoiceVideoProcessedMessage
        {
            SessionId = Guid.NewGuid(),
            Status = text,
            Timestamp = DateTime.UtcNow
        };

        // 2. Vi bruger vores abstrakte klient til at sende beskeden ud
        await _messageClient.PublishAsync(message);

        return Ok(new { Status = "Besked sendt til RabbitMQ!", Data = message });
    }
}