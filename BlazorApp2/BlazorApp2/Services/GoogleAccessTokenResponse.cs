using System.Text.Json.Serialization;

namespace BlazorApp2.Services;

public class GoogleAccessTokenResponse
{
    /// <summary>
    /// Initial token used to gain access
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    /// <summary>
    /// Use to get new token
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }
    /// <summary>
    /// Measured in seconds
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    /// <summary>
    /// Should always be "Bearer"
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
}