using Jobs.Common.Responses;
using Jobs.Entities.DataModel;
using Jobs.Entities.Responses;

namespace Jobs.AccountApi.Contracts;

public interface IKeycloakAccountService
{
    Task<KeycloakTokenResponse> LoginAsync(string username, string password);
    Task<RegisterUserResponse> RegisterUser(User user);
    Task<bool> LogoutAsync(LogoutUser user);
    Task<KeycloakRespone?> RefreshTokenAsync(string refreshToken);
}