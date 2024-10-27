using Application.Clients;
using Application.DTOs;
using Application.UseCases;
using Application.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class KeyController : ControllerBase
{
    private readonly ILogger<KeyController> _logger;
    private readonly UCGenerateKey _ucGenerateKey;
    private readonly IAuthServerClient _authServerClient;

    public KeyController(ILogger<KeyController> logger, UCGenerateKey ucGenerateKey, IAuthServerClient authServerClient)
    {
        _logger = logger;
        _ucGenerateKey = ucGenerateKey;
        _authServerClient = authServerClient;
    }

    [HttpPost]
    public async Task<ActionResult<string>> GenerateKey(string jwt)
    {
        _logger.LogDebug("KeyController - GenerateKey");

        if (await _authServerClient.IsAuthenticated(jwt))
        {
            var userId = JwtParser.ParseJwt(jwt);
            (var Success, var Message) = await _ucGenerateKey.GenerateKey(userId);
            if (Success) return Ok(Message); // HTTP 200
            return BadRequest(Message);
        }

        return Unauthorized();
    }
}