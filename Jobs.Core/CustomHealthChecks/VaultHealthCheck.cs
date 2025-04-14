using System.Net.Http.Json;
using Jobs.Core.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Jobs.Core.CustomHealthChecks;

public class VaultHealthCheck(IHttpClientFactory httpClientFactory, IOptions<VaultOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var uri =
            $"{options.Value.VaultServerUrl}:{options.Value.VaultServerPort}/{options.Value.VaultHealthCheckUriPart.TrimStart('/')}";
        
        Console.WriteLine($"Vault Uri: {uri}");
        
        var responseResult = await httpClient.GetFromJsonAsync<VaultHealthCheckResponse>(uri, cancellationToken: cancellationToken);

        if (responseResult is { Initialized: true, Sealed: false })
        {
            return HealthCheckResult.Healthy($"Vault endpoints is healthy.");
        }

        return HealthCheckResult.Unhealthy("Vault endpoint is unhealthy");
    }
}