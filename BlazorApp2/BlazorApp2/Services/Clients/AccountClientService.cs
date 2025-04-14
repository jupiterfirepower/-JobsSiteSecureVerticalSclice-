using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorApp2.Contracts.Clients;
using BlazorApp2.Settings;
using Jobs.Common.Constants;
using Jobs.Common.Helpers;
using Jobs.Common.Responses;
using Jobs.Common.SerializationSettings;
using Jobs.Entities.DataModel;
using Jobs.Entities.Responses;
using Microsoft.Extensions.Options;

namespace BlazorApp2.Services.Clients;

public class AccountClientService(HttpClient client, IOptions<ServicesSettings> settings) : IAccountClientService
{
    private string? _lastApiKey;
    
    private readonly JsonSerializerOptions _jsonSerializerOptions = JsonSerializerSetting.JsonSerializerOptions;

    public async Task<RegisterUserResponse> RegisterAsync(User user)
    {
        client.DefaultRequestVersion = HttpVersion.Version30;
        client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;

        HttpClientHeaderHelper.SetHttpClientSecurityHeaders(client, _lastApiKey ?? SecureApiKey.AccountFirstApiKey, SecureApiKey.AccountSecretKey);
        Console.WriteLine($"{nameof(AccountClientService)} Last Api Key - {_lastApiKey}");
        var data = JsonSerializer.Serialize(user);
        
        var content = new StringContent(data, Encoding.UTF8, HttpHeaderKeys.AppJsonMediaTypeValue);
       
        var response = await client.PostAsync(settings.Value.AccountApiRegisterUrl , content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        
        _lastApiKey = response.Headers.GetValues(HttpHeaderKeys.XApiHeaderKey).FirstOrDefault();
        Console.WriteLine($"{nameof(AccountClientService)} Response Api Key - {_lastApiKey}");
        
        var result = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RegisterUserResponse>(result, _jsonSerializerOptions)!;
    }

    public async Task<KeycloakTokenResponse?> LoginAsync(string username, string password)
    {
        client.DefaultRequestVersion = HttpVersion.Version30;
        client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
        
        HttpClientHeaderHelper.SetHttpClientSecurityHeaders(client, _lastApiKey ?? SecureApiKey.AccountFirstApiKey, SecureApiKey.AccountSecretKey);

        var user = new LoginUser { UserName = username, Password = password };
        var data = JsonSerializer.Serialize(user);

        var content = new StringContent(data, Encoding.UTF8, HttpHeaderKeys.AppJsonMediaTypeValue);
        
        var adminResponse = await client.PostAsync(settings.Value.AccountApiLoginUrl, 
                content)
            .ConfigureAwait(false);
        adminResponse.EnsureSuccessStatusCode();
        
        _lastApiKey = adminResponse.Headers.GetValues(HttpHeaderKeys.XApiHeaderKey).FirstOrDefault();

        var result = await adminResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<KeycloakTokenResponse>(result);
    }
    
    public async Task<bool> LogoutAsync(LogoutUser user)
    {
        client.DefaultRequestVersion = HttpVersion.Version30;
        client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
        
        HttpClientHeaderHelper.SetHttpClientSecurityHeaders(client, _lastApiKey ?? SecureApiKey.AccountFirstApiKey, SecureApiKey.AccountSecretKey);

        var data = JsonSerializer.Serialize(user);
        
        var content = new StringContent(data, Encoding.UTF8, HttpHeaderKeys.AppJsonMediaTypeValue);
        
        var response = await client.PostAsync(settings.Value.AccountApiLogoutUrl, content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        
        _lastApiKey = response.Headers.GetValues(HttpHeaderKeys.XApiHeaderKey).FirstOrDefault();
        
        return true;
    }
    
    public async Task<KeycloakTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        client.DefaultRequestVersion = HttpVersion.Version30;
        client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
        
        HttpClientHeaderHelper.SetHttpClientSecurityHeaders(client, _lastApiKey ?? SecureApiKey.AccountFirstApiKey, SecureApiKey.AccountSecretKey);

        var data = JsonSerializer.Serialize(new
        {
            refreshToken = refreshToken
        }, _jsonSerializerOptions);
        
        var content = new StringContent(data, Encoding.UTF8, HttpHeaderKeys.AppJsonMediaTypeValue);
        
        var response = await client.PostAsync(settings.Value.AccountApiRefreshTokenUrl, content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        
        _lastApiKey = response.Headers.GetValues(HttpHeaderKeys.XApiHeaderKey).FirstOrDefault();
        
        var result = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<KeycloakTokenResponse>(result);
    }
}