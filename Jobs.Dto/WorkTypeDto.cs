using System.Text.Json.Serialization;

namespace Jobs.DTO;

public class WorkTypeDto
{
    [JsonPropertyName("workTypeId")]
    public int WorkTypeId { get; set; }
    
    [JsonPropertyName("workTypeName")]
    public string WorkTypeName { get; set; }
    
    [JsonPropertyName("created")]
    public DateTime Created { get; init; } 
    
    [JsonPropertyName("modified")]
    public DateTime Modified { get; set; } 
}