using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Application.Clients;
using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Application.Utils;
using Domain.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.UseCases;

[TestSubject(typeof(UCProcessMessage))]
public class UCProcessMessageTest
{
    private readonly Mock<ILogger<UCProcessMessage>> _loggerMock = new();
    private readonly Mock<IKeyPairRepository> _keyPairRepositoryMock = new();
    private readonly Mock<IUserVoteRepository> _userVoteRepositoryMock = new();
    private readonly Mock<IVoteServerClient> _voteServerClientMock = new();
    private readonly Mock<IRSAUtil> _rsaUtilMock = new();
    private readonly UCProcessMessage _ucProcessMessage;

    public UCProcessMessageTest()
    {
        _ucProcessMessage = new UCProcessMessage(_loggerMock.Object, _keyPairRepositoryMock.Object,
            _userVoteRepositoryMock.Object, _voteServerClientMock.Object, _rsaUtilMock.Object);
    }

    [Fact]
    public async Task ProcessMessage_ReturnsSuccess_WhenMessageProcessedSuccessfully()
    {
        var userMessageDto = new UserMessageDTO("encryptedMessage", "testUserId");
        var keyPair = new KeyPair("testUserId", "publicKey", "privateKey") { CreatedAt = DateTime.UtcNow };
        var signatureMessageDto = new SignatureMessageDTO("opt1", Guid.NewGuid().ToString());
        var voteResponse = (Success: true, Message: "Vote sent successfully");

        _keyPairRepositoryMock.Setup(repo => repo.FilterAsync(It.IsAny<Expression<Func<KeyPair, bool>>>()))
            .ReturnsAsync(new List<KeyPair> { keyPair });
        _rsaUtilMock.Setup(util => util.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns(signatureMessageDto);
        _userVoteRepositoryMock.Setup(repo => repo.FindByAsync(It.IsAny<Expression<Func<UserVote, bool>>>()))
            .ReturnsAsync((UserVote)null);
        _voteServerClientMock.Setup(client => client.SendMessage(It.IsAny<SignatureMessageDTO>()))
            .ReturnsAsync(voteResponse);

        var result = await _ucProcessMessage.ProcessMessage(userMessageDto);

        Assert.True(result.Success);
        Assert.Equal("Stimme erfolgreich gesendet", result.Message);
    }

    [Fact]
    public async Task ProcessMessage_ReturnsError_WhenKeyPairNotFound()
    {
        var userMessageDto = new UserMessageDTO("encryptedMessage", "testUserId");

        _keyPairRepositoryMock.Setup(repo => repo.FilterAsync(It.IsAny<Expression<Func<KeyPair, bool>>>()))
            .ReturnsAsync(new List<KeyPair>());

        var result = await _ucProcessMessage.ProcessMessage(userMessageDto);

        Assert.False(result.Success);
        Assert.Equal("Schlüsselpaar nicht gefunden", result.Message);
    }

    [Fact]
    public async Task ProcessMessage_ReturnsError_WhenDecryptionFails()
    {
        var userMessageDto = new UserMessageDTO("encryptedMessage", "testUserId");
        var keyPair = new KeyPair("testUserId", "publicKey", "privateKey") { CreatedAt = DateTime.UtcNow };

        _keyPairRepositoryMock.Setup(repo => repo.FilterAsync(It.IsAny<Expression<Func<KeyPair, bool>>>()))
            .ReturnsAsync(new List<KeyPair> { keyPair });
        _rsaUtilMock.Setup(util => util.Decrypt(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException());

        var result = await _ucProcessMessage.ProcessMessage(userMessageDto);

        Assert.False(result.Success);
        Assert.Equal("Fehler beim Entschlüsseln der Nachricht", result.Message);
    }

    [Fact]
    public async Task ProcessMessage_ReturnsError_WhenUserHasAlreadyVoted()
    {
        var userMessageDto = new UserMessageDTO("encryptedMessage", "testUserId");
        var keyPair = new KeyPair("testUserId", "publicKey", "privateKey") { CreatedAt = DateTime.UtcNow };
        var signatureMessageDto = new SignatureMessageDTO("opt1", "testSurveyId");
        var existingVote = new UserVote("testUserId", "testSurveyId");

        _keyPairRepositoryMock.Setup(repo => repo.FilterAsync(It.IsAny<Expression<Func<KeyPair, bool>>>()))
            .ReturnsAsync(new List<KeyPair> { keyPair });
        _rsaUtilMock.Setup(util => util.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns(signatureMessageDto);
        _userVoteRepositoryMock.Setup(repo => repo.FindByAsync(It.IsAny<Expression<Func<UserVote, bool>>>()))
            .ReturnsAsync(existingVote);

        var result = await _ucProcessMessage.ProcessMessage(userMessageDto);

        Assert.False(result.Success);
        Assert.Equal("Du hast bereits für diese Wahl abgestimmt", result.Message);
    }

    [Fact]
    public async Task ProcessMessage_ReturnsError_WhenVoteServerClientFails()
    {
        var userMessageDto = new UserMessageDTO("encryptedMessage", "testUserId");
        var keyPair = new KeyPair("testUserId", "publicKey", "privateKey") { CreatedAt = DateTime.UtcNow };
        var signatureMessageDto = new SignatureMessageDTO("opt1", "testSurveyId");
        var voteResponse = (Success: false, Message: "Vote server error");

        _keyPairRepositoryMock.Setup(repo => repo.FilterAsync(It.IsAny<Expression<Func<KeyPair, bool>>>()))
            .ReturnsAsync(new List<KeyPair> { keyPair });
        _rsaUtilMock.Setup(util => util.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns(signatureMessageDto);
        _userVoteRepositoryMock.Setup(repo => repo.FindByAsync(It.IsAny<Expression<Func<UserVote, bool>>>()))
            .ReturnsAsync((UserVote)null);
        _voteServerClientMock.Setup(client => client.SendMessage(It.IsAny<SignatureMessageDTO>()))
            .ReturnsAsync(voteResponse);

        var result = await _ucProcessMessage.ProcessMessage(userMessageDto);

        Assert.False(result.Success);
        Assert.Equal("Vote server error", result.Message);
    }
}