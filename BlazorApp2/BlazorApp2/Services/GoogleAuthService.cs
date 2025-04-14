using System.Net.Http.Headers;
using System.Text.Json;
using BlazorApp2.Contracts;
using BlazorApp2.Services;

namespace BlazorApp2.Services;

public class GoogleAuthService : IGoogleAuthService
{
    public async Task<GoogleAccessTokenResponse> GetAccessToken(string code)
    {
        var httpClient = new HttpClient();
        
        // Form data is typically sent as key-value pairs
        var adminData = new List<KeyValuePair<string, string>>
        {
            new ("client_id", "apps.googleusercontent.com"),
            new ("client_secret", ""),
            new ("code", code),
            new ("grant_type", "authorization_code"),
            new ("access_type", "online"),
            new ("redirect_uri", "http://localhost:5047/oauth"),
            new ("scope", "https://www.googleapis.com/auth/userinfo.profile")
        };

        // Encodes the key-value pairs for the ContentType 'application/x-www-form-urlencoded'
        HttpContent adminContent = new FormUrlEncodedContent(adminData);
        
        //var adminResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", adminContent);
        var adminResponse = await httpClient.PostAsync("https://www.googleapis.com/oauth2/v4/token", adminContent);
        adminResponse.EnsureSuccessStatusCode();

        var result = await adminResponse.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<GoogleAccessTokenResponse>(result);
        
        return data;
    }
    
    public async Task<string> GetUserInfo(string accessToken)
    {
        var httpClient = new HttpClient();
        
        
        httpClient.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        
        var adminResponse = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
        //var adminResponse = await httpClient.GetAsync("https://www.googleapis.com/plus/v1/people/me");
        adminResponse.EnsureSuccessStatusCode();

        var result = await adminResponse.Content.ReadAsStringAsync();
        //var data = JsonSerializer.Deserialize<GoogleAccessTokenResponse>(result);
        
        return result;
    }
    
    //
}