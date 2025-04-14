using System.Text.Json.Serialization;

namespace Jobs.Common.DataModel;

public class ApiKey
{
    [JsonPropertyName("i")]
    public int Id { get; set; }
    
    [JsonPropertyName("k")]
    public string Key { get; set; }
    
    [JsonPropertyName("e")]
    public DateTime? Expiration { get; set; }
}