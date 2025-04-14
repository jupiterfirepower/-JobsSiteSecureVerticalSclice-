using System.Text.Json.Serialization;

namespace Jobs.Core.CustomHealthChecks;

public record KeycloakHealthCheckResponse([property: JsonPropertyName("status")] string Status);
