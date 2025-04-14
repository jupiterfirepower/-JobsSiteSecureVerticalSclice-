using System.Text.Json.Serialization;

namespace Jobs.Dto.Request;

public record LoginUserDto(
    [property: JsonPropertyName("username")]
    string Username,
    [property: JsonPropertyName("password")]
    string Password );
