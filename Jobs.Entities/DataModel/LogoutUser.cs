using System.Text.Json.Serialization;

namespace Jobs.Entities.DataModel;

public class LogoutUser
{
    [JsonPropertyName("username")]
    public string Username { get; set; }
}