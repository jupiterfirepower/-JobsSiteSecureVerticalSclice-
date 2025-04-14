using System.Text.Json.Serialization;

namespace Jobs.Dto.Request;

public record LogoutUserDto(
    [property: JsonPropertyName("username")]
    string Username);
