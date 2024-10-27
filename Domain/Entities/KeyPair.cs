namespace Domain.Entities;

public class KeyPair : _BaseEntity
{
    public string UserId { get; set; }
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }

    public Guid Guid => _guid;

    public KeyPair(string userId, string publicKey, string privateKey)
    {
        UserId = userId;
        PrivateKey = privateKey;
        PublicKey = publicKey;
    }
}