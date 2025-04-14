using System.Text.Json.Serialization;

namespace BlazorApp2.Components.Shared.Data;

public class CompanyData
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("note")]
    public required string Note { get; set; }
    
    [JsonPropertyName("link")]
    public string? Link { get; set; }
}