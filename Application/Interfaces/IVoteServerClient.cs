using Application.DTOs;

namespace Application.Clients;

public interface IVoteServerClient
{
    Task<(bool Success, string Message)> SendMessage(SignatureMessageDTO signatureMessageDto);
}