using Application.DTOs;

namespace Application.Utils;

public interface IRSAUtil
{
    (string PublicKey, string PrivateKey) GenerateKeyPair();
    SignatureMessageDTO Decrypt(string privateKey, string data);
}