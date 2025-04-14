using System.Text.Json.Serialization;

namespace Jobs.Entities.DataModel;

public class LoginUser
{
    [JsonPropertyName("username")]
    public string UserName { get; set; }
    [JsonPropertyName("password")]
    public string Password { get; set; }
}