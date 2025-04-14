using System.Text.Json.Serialization;

namespace BlazorApp2.Components.Shared.Data;

public class UserData
{
    [JsonPropertyName("email")]
    public required string Email { get; set; } = String.Empty;

    [JsonPropertyName("password")] public required string Password { get; set; } = String.Empty;
}