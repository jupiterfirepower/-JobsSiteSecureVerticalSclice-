using BlazorApp2.Services;

namespace BlazorApp2.Contracts;

public interface IGoogleAuthService
{
    Task<GoogleAccessTokenResponse> GetAccessToken(string code);
}