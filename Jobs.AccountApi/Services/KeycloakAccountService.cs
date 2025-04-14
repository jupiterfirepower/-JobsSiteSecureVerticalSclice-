using System.Net.Http.Headers;
using System.Text.Json;
using Jobs.Common.Responses;
using Jobs.Common.SerializationSettings;
using Keycloak.Client.Models;
using Serilog;

namespace Jobs.AccountApi.Services;

public class KeycloakAccountService(IHttpClientFactory httpClientFactory, 
    string baseUrl = "http://localhost:9001", 
    string realm = "mjobs", 
    string adminUserName = "admin", 
    string adminPassword = "newpwd")
{
    protected async Task<KeycloakRespone?> GetKeycloakAccessToken()
    {
        using var httpClient = httpClientFactory.CreateClient();

        // Form data is typically sent as key-value pairs
        var adminData = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("client_id", "admin-cli"),
            new("username", adminUserName),  //"admin"
            new("password", adminPassword), //"newpwd"
        };

        // Encodes the key-value pairs for the ContentType 'application/x-www-form-urlencoded'
        using var adminContent = new FormUrlEncodedContent(adminData);

        var adminResponse =
            await httpClient.PostAsync($"{baseUrl.TrimEnd('/')}/realms/master/protocol/openid-connect/token",
                adminContent);
        adminResponse.EnsureSuccessStatusCode();

        var result = await adminResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<KeycloakRespone>(result);
    }

    protected async Task<UserRepresentation?> GetUserAsync(string accessToken, string userName)
    {
        using var client = httpClientFactory.CreateClient();
        
        client.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer",  accessToken);
      
        var response = await client.GetAsync($"{baseUrl.TrimEnd('/')}/admin/realms/{realm}/users?username={userName}").ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
        var resultUser = await response.Content.ReadAsStringAsync();
        
        Log.Information($"User - {resultUser}");
        var userId = resultUser.Length > 2 ? resultUser.Substring(8, 36) : string.Empty;
        Log.Information($"UserId - {userId}");
        
        var userInfo = JsonSerializer.Deserialize<List<UserRepresentation>>(resultUser, JsonSerializerSetting.JsonSerializerOptions);
        Log.Information($"userInfo - {userInfo != null}, count - {userInfo?.Count}");
        
        return userInfo?.FirstOrDefault();
    }
}