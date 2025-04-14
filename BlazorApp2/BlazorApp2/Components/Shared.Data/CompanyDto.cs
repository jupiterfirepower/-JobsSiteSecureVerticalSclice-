using System.Text.Json.Serialization;

namespace BlazorApp2.Components.Shared.Data;

public class CompanyDto
{
    [JsonPropertyName("companyId")]
    public int CompanyId { get; set; }
    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; }
    [JsonPropertyName("companyDescription")]
    public string CompanyDescription { get; set; }
    [JsonPropertyName("companyLogoPath")]
    public string CompanyLogoPath { get; set; }
    [JsonPropertyName("companyLink")]
    public string companyLink { get; set; }
    [JsonPropertyName("isVisible")]
    public bool isVisible { get; set; }
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}