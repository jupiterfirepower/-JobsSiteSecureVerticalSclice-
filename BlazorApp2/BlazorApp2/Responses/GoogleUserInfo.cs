using System.Text.Json.Serialization;

namespace BlazorApp2.Responses;

public class GoogleUserInfo
{
    [JsonPropertyName("sub")]
    public required string Sub { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("given_name")]
    public required string GivenName { get; set; }
    [JsonPropertyName("family_name")]
    public required string FamilyName { get; set; }
    [JsonPropertyName("picture")]
    public required string Picture { get; set; }
    [JsonPropertyName("email")]
    public required string Email { get; set; }
    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }
}