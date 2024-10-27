using Application.Clients;
using Application.DTOs;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;


namespace Presentation.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class MessageController : ControllerBase
{
    private readonly ILogger<MessageController> _logger;
    private readonly UCProcessMessage _ucProcessMessage;
    private readonly IAuthServerClient _authServerClient;

    public MessageController(ILogger<MessageController> logger, UCProcessMessage ucProcessMessage,
        IAuthServerClient authServerClient)
    {
        _logger = logger;
        _ucProcessMessage = ucProcessMessage;
        _authServerClient = authServerClient;
    }

    [HttpPost]
    public async Task<ActionResult<string>> ProcessMessage([FromQuery] string jwt,
        [FromBody] UserMessageDTO userMessageDto)
    {
        _logger.LogDebug("MessageController - ProcessMessage");

        if (await _authServerClient.IsAuthenticated(jwt))
        {
            _logger.LogInformation("MessageController - ProcessMessage - User authenticated");
            _logger.LogDebug(userMessageDto.ToString());
            (var Success, var Message) = await _ucProcessMessage.ProcessMessage(userMessageDto);
            if (Success) return Ok(Message); // HTTP 200
            return BadRequest(Message); // HTTP 400    
        }

        return Unauthorized();
    }
}