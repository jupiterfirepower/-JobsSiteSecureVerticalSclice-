using System.Text.Json.Serialization;

namespace Jobs.DTO;

public record CompanyDto(
    [property: JsonPropertyName("companyId")]
    int CompanyId,
    [property: JsonPropertyName("companyName")]
    string CompanyName,
    [property: JsonPropertyName("companyDescription")]
    string CompanyDescription,
    [property: JsonPropertyName("companyLogoPath")]
    string CompanyLogoPath,
    [property: JsonPropertyName("companyLink")]
    string CompanyLink,
    [property: JsonPropertyName("isVisible")]
    bool IsVisible,
    [property: JsonPropertyName("isActive")]
    bool IsActive);