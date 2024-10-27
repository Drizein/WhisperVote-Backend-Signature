using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.UseCases;
using Application.Utils;
using Domain.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.UseCases;

[TestSubject(typeof(UCGenerateKey))]
public class UCGenerateKeyTest
{
    private readonly Mock<ILogger<UCGenerateKey>> _loggerMock = new();
    private readonly Mock<IKeyPairRepository> _keyPairRepositoryMock = new();
    private readonly Mock<IRSAUtil> _rsaUtilMock = new();
    private readonly UCGenerateKey _ucGenerateKey;

    public UCGenerateKeyTest()
    {
        _ucGenerateKey = new UCGenerateKey(_loggerMock.Object, _keyPairRepositoryMock.Object, _rsaUtilMock.Object);
    }

    [Fact]
    public async Task GenerateKey_ReturnsPublicKey_WhenKeyPairIsGenerated()
    {
        var userId = Guid.NewGuid().ToString();
        _keyPairRepositoryMock.Setup(repo => repo.Add(It.IsAny<KeyPair>()));
        _keyPairRepositoryMock.Setup(repo => repo.SaveChangesAsync());
        _rsaUtilMock.Setup(util => util.GenerateKeyPair()).Returns(("publicKey", "privateKey"));

        var result = await _ucGenerateKey.GenerateKey(userId);

        Assert.True(result.Success);
        Assert.NotNull(result.Message);
        Assert.Equal("publicKey", result.Message);
        _keyPairRepositoryMock.Verify(repo => repo.Add(It.IsAny<KeyPair>()), Times.Once);
        _keyPairRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GenerateKey_ThrowsException_WhenGeneratingKeyPairFails()
    {
        var userId = "testUserId";
        _keyPairRepositoryMock.Setup(repo => repo.Add(It.IsAny<KeyPair>()));
        _rsaUtilMock.Setup(util => util.GenerateKeyPair()).Throws(new CryptographicException("Test exception"));

        var result = await _ucGenerateKey.GenerateKey(userId);

        Assert.False(result.Success);
        Assert.Equal("Fehler beim Generieren des Schlüsselpaares", result.Message);
    }
}