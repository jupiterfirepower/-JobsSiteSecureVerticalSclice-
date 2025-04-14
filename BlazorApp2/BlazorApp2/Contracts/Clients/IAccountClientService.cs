using Jobs.Common.Responses;
using Jobs.Entities.DataModel;
using Jobs.Entities.Responses;

namespace BlazorApp2.Contracts.Clients;

public interface IAccountClientService
{
    Task<RegisterUserResponse> RegisterAsync(User user);
    Task<KeycloakTokenResponse?> LoginAsync(string username, string password);
    Task<bool> LogoutAsync(LogoutUser user);
    
    Task<KeycloakTokenResponse> RefreshTokenAsync(string refreshToken);
}