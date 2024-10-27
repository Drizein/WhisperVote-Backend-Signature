using Application.Clients;
using Application.DTOs;
using Application.Interfaces;
using Application.Utils;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.UseCases;

public class UCProcessMessage
{
    private readonly ILogger<UCProcessMessage> _logger;
    private readonly IKeyPairRepository _keyPairRepository;
    private readonly IUserVoteRepository _userVoteRepository;
    private readonly IVoteServerClient _voteServerClient;
    private readonly IRSAUtil _rsaUtil;

    public UCProcessMessage(ILogger<UCProcessMessage> logger, IKeyPairRepository keyPairRepository,
        IUserVoteRepository userVoteRepository, IVoteServerClient voteServerClient, IRSAUtil rsaUtil)
    {
        _logger = logger;
        _keyPairRepository = keyPairRepository;
        _userVoteRepository = userVoteRepository;
        _voteServerClient = voteServerClient;
        _rsaUtil = rsaUtil;
    }

    public async Task<(bool Success, string Message)> ProcessMessage(UserMessageDTO userMessageDto)
    {
        _logger.LogInformation("UCReceiveMessage - ReceiveMessage");


        KeyPair keyPair;
        try
        {
            keyPair =
                (await _keyPairRepository.FilterAsync(x => x.UserId == userMessageDto.userId))
                .OrderByDescending(k => k.CreatedAt).FirstOrDefault() ?? throw new InvalidOperationException();
        }
        catch (Exception e)
        {
            _logger.LogError("KeyPair not found: {}", e);
            return (false, "Schlüsselpaar nicht gefunden");
        }

        _logger.LogDebug("KeyPair found and PubKey is: {} {} {}", keyPair.Guid, keyPair.PublicKey, keyPair.CreatedAt);

        SignatureMessageDTO signatureMessageDto;
        try
        {
            signatureMessageDto = _rsaUtil.Decrypt(keyPair.PrivateKey, userMessageDto.message);
        }
        catch (Exception e)
        {
            return (false, "Fehler beim Entschlüsseln der Nachricht");
        }

        _logger.LogDebug(signatureMessageDto.ToString());

        if (await _userVoteRepository.FindByAsync(x =>
                x.UserId == userMessageDto.userId && x.SurveyId == signatureMessageDto.surveyId) !=
            null) return (false, "Du hast bereits für diese Wahl abgestimmt");

        _userVoteRepository.Add(new UserVote(userMessageDto.userId, signatureMessageDto.surveyId));

        (bool Success, string Message) voteResponse = await _voteServerClient.SendMessage(signatureMessageDto);

        if(!voteResponse.Success) return (false, voteResponse.Message);

        await _userVoteRepository.SaveChangesAsync();
        return (true, "Stimme erfolgreich gesendet");
    }
}