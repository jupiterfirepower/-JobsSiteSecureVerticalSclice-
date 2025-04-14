using Jobs.Common.Responses;
using Jobs.Entities.Responses;

namespace BlazorApp2.Contracts;

public interface IAccountService
{
    bool IsLoggedIn { get; }
    string Username { get; }
    public Task<bool> LoginAsync(string username, string password);
    Task<RegisterUserResponse> RegisterAsync(string email, string password, string firstName, string lastName);
    public Task<bool> LogoutAsync();

    Task<KeycloakTokenResponse> RefreshTokenAsync(string refreshToken);
}