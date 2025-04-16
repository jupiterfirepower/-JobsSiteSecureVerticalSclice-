using Jobs.Core.CustomHealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Jobs.AccountApi.Extentions;

public static class ConfigureHealthChecksExtension
{
    public static void ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine($"Connection string: {connectionString}");
        
        var consulHost = configuration["ConsulSetting:ConsulHost"];
        var consulPort = int.Parse(configuration["ConsulSetting:ConsulPort"]!);
        var consulRequireHttps = bool.Parse(configuration["ConsulSetting:ConsulRequireHttps"]!);
        
        // Add HealthChecks
        services.AddHealthChecks()
            .AddCheck<MemoryHealthCheck>($"Account Service Memory Check", failureStatus: HealthStatus.Unhealthy, tags:
                ["Account Service"])
            .AddCheck<VaultHealthCheck>("Vault Check", failureStatus: HealthStatus.Unhealthy, tags:
                ["Hashicorp Vault"])
            .AddCheck<KeycloakHealthCheck>("Keycloak Check", failureStatus: HealthStatus.Unhealthy, tags:
                ["IBM Keycloak"])
            .AddConsul(option =>
            {
                option.HostName = consulHost!;
                option.Port = consulPort;
                option.RequireHttps = consulRequireHttps;
            }, tags: ["Consul"]);
    
        services.AddHealthChecksUI(opt => {
                opt.SetEvaluationTimeInSeconds(60); //time in seconds between check    
                opt.MaximumHistoryEntriesPerEndpoint(30); //maximum history of checks    
                opt.SetApiMaxActiveRequests(1); //api requests concurrency    
            })
            .AddInMemoryStorage();
    }
}