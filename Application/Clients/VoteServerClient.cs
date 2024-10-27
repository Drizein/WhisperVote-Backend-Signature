using System.Text;
using System.Text.Json;
using Application.DTOs;

namespace Application.Clients;

public class VoteServerClient : IVoteServerClient
{
    private static readonly string Url = Environment.GetEnvironmentVariable("ConnectionStrings__VoteServer")!;

    public async Task<(bool Success, string Message)> SendMessage(SignatureMessageDTO signatureMessageDto)
    {
        using var httpClient = new HttpClient();
        var url = $"{Url}/Survey/Vote";
        var jsonContent = JsonSerializer.Serialize(signatureMessageDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var responseMessage = await response.Content.ReadAsStringAsync();
            return (true, responseMessage);
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        return (false, errorMessage);
    }
}