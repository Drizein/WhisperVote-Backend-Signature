namespace Application.Clients;

public interface IAuthServerClient
{
    Task<bool> IsAuthenticated(string jwt);
}