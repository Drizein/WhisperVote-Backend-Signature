using Application.Interfaces;
using Application.Utils;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.UseCases;

public class UCGenerateKey
{
    private readonly ILogger<UCGenerateKey> _logger;
    private readonly IKeyPairRepository _keyPairRepository;
    private readonly IRSAUtil _rsaUtil;

    public UCGenerateKey(ILogger<UCGenerateKey> logger, IKeyPairRepository keyPairRepository, IRSAUtil rsaUtil)
    {
        _logger = logger;
        _keyPairRepository = keyPairRepository;
        _rsaUtil = rsaUtil;
    }

    public async Task<(bool Success, string Message)> GenerateKey(string userId)
    {
        _logger.LogDebug("UCGenerateKey - GenerateKey");
        string publicKey, privateKey;
        try
        {
            (publicKey, privateKey) = _rsaUtil.GenerateKeyPair();
        }
        catch (Exception e)
        {
            _logger.LogError("Error generating key pair: {}", e);
            return (false, "Fehler beim Generieren des Schlüsselpaares");
        }

        var keyPair = new KeyPair(userId, publicKey, privateKey);

        _keyPairRepository.Add(keyPair);
        await _keyPairRepository.SaveChangesAsync();

        return (true, keyPair.PublicKey);
    }
}