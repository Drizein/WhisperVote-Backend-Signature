using System.Security.Cryptography;
using System.Text;
using Application.DTOs;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Application.Utils;

public class RSAUtil : IRSAUtil
{
    private readonly ILogger<RSAUtil> _logger;

    public RSAUtil(ILogger<RSAUtil> logger)
    {
        _logger = logger;
    }

    public (string PublicKey, string PrivateKey) GenerateKeyPair()
    {
        using var rsa = new RSACryptoServiceProvider(4096);
        return (Convert.ToBase64String(rsa.ExportRSAPublicKey()), Convert.ToBase64String(rsa.ExportRSAPrivateKey()));
    }

    public SignatureMessageDTO Decrypt(string privateKey, string data)
    {
        _logger.LogDebug("RSAUtil - Decrypt");
        _logger.LogDebug(data);
        _logger.LogDebug("Private Key is: {}", privateKey);

        using var rsa = new RSACryptoServiceProvider(4096);
        var privateKeyBytes = Convert.FromBase64String(privateKey);
        var dataBytes = Convert.FromBase64String(data);

        _logger.LogDebug("After data conversing to bytes with result: {}", dataBytes.ToString());

        rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
        byte[] decryptedBytes;
        try
        {
            decryptedBytes = rsa.Decrypt(dataBytes, true);
        }
        catch (Exception e)
        {
            _logger.LogError("Error decrypting: {}", e.Message);
            throw;
        }

        var decryptedString = Encoding.UTF8.GetString(decryptedBytes);

        _logger.LogDebug("After decrypting with result: {}", decryptedString);

        return JsonSerializer.Deserialize<SignatureMessageDTO>(decryptedString);
    }
}