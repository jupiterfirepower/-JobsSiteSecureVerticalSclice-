using System.Text.Json.Serialization;

namespace Jobs.Common.Responses;

public class KeycloakUserInfoResponse
{
    /*[JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; }
    [JsonPropertyName("lastName")]
    public string LastName { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
    [JsonPropertyName("totp")]
    public bool Totp { get; set; }*/
    [JsonPropertyName("id")]
    public string Id { get; set; }
    public string username { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string email { get; set; }
    public bool emailVerified { get; set; }
    public object createdTimestamp { get; set; }
    public bool enabled { get; set; }
    public bool totp { get; set; }
    public List<object> disableableCredentialTypes { get; set; }
    public List<object> requiredActions { get; set; }
    public int notBefore { get; set; }
    public Access access { get; set; }
}