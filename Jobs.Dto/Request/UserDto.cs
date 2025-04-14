using System.Text.Json.Serialization;

namespace Jobs.Dto.Request;

public record UserDto(
    [property: JsonPropertyName("email")] 
    string Email,
    [property: JsonPropertyName("password")]
    string Password,
    [property: JsonPropertyName("firstname")]
    string FirstName = "",
    [property: JsonPropertyName("lastname")]
    string LastName = ""
);
