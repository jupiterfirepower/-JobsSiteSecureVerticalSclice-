using System.Net.Http.Json;
using Jobs.Core.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Jobs.Core.CustomHealthChecks;

public class KeycloakHealthCheck(IHttpClientFactory httpClientFactory, IOptions<KeycloakOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var uri =
            $"{options.Value.KeycloakServerUrl}:{options.Value.KeycloakServerPort}/{options.Value.KeycloakHealthCheckUriPart.TrimStart('/')}";
        
        Console.WriteLine($"Keycloak Uri: {uri}");
        
        var responseResult = await httpClient.GetFromJsonAsync<KeycloakHealthCheckResponse>(uri, cancellationToken: cancellationToken);

        if (responseResult is { Status: "UP" })
        {
            return HealthCheckResult.Healthy($"Keycloak endpoints is healthy.");
        }

        return HealthCheckResult.Unhealthy("Keycloak endpoint is unhealthy");
    }
}