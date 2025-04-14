using System.Text.Json.Serialization;

namespace Jobs.Core.DataModel;

public class ApiKey
{
    [JsonPropertyName("k")]
    public string Key { get; set; }
    
    [JsonPropertyName("e")]
    public DateTime? Expiration { get; set; }
}