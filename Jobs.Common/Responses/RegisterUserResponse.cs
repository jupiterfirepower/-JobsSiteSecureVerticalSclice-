using System.Text.Json.Serialization;
using Keycloak.Client.Models;

namespace Jobs.Common.Responses;

public class RegisterUserResponse
{
    [JsonPropertyName("result")]
    public bool Result { get; set; }
    [JsonPropertyName("keycloak_user")]
    public bool IsAdded { get; set; }
}