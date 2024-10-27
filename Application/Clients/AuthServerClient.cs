namespace Application.Clients;

public class AuthServerClient : IAuthServerClient
{
    private static readonly string Url = Environment.GetEnvironmentVariable("ConnectionStrings__AuthServer")!;

    public async Task<bool> IsAuthenticated(string jwt)
    {
        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");

        var response = await httpClient.GetAsync(Url + "/Auth/ValidateToken");

        return response.IsSuccessStatusCode;
    }
}