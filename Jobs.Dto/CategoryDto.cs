using System.Text.Json.Serialization;

namespace Jobs.DTO;

public record CategoryDto(
    [property: JsonPropertyName("categoryId")]
    int CategoryId,
    [property: JsonPropertyName("categoryName")]
    string CategoryName,
    [property: JsonPropertyName("parentId")]
    int? ParentId,
    [property: JsonPropertyName("isVisible")]
    bool IsVisible,
    [property: JsonPropertyName("isActive")]
    bool IsActive,
    [property: JsonPropertyName("created")]
    DateTime Created,
    [property: JsonPropertyName("modified")]
    DateTime Modified);
