using System.Text.Json.Serialization;

namespace Jobs.Entities.DataModel;

public class Credentials
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "password";

    [JsonPropertyName("value")] 
    public string Value { get; set; }

    [JsonPropertyName("temporary")]
    public bool Temporary { get; set; } = false;
}

public class UserContext(User user)
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = user.Email;

    [JsonPropertyName("username")]
    public string UserName { get; set; } = user.Email;
    
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } 
    
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } 

    [JsonPropertyName("enabled")] 
    public string Enabled { get; set; } = "true";
    
    [JsonPropertyName("credentials")] 
    public Credentials[] Credentials { get; set; }
}