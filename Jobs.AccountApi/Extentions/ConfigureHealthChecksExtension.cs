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
        //.AddUrlGroup(new Uri("https://localhost:7111/api/v1/heartbeats/ping"), name: "base URL", failureStatus: HealthStatus.Unhealthy);
    
        services.AddHealthChecksUI(opt => {
                opt.SetEvaluationTimeInSeconds(60); //time in seconds between check    
                opt.MaximumHistoryEntriesPerEndpoint(30); //maximum history of checks    
                opt.SetApiMaxActiveRequests(1); //api requests concurrency    
                //opt.AddHealthCheckEndpoint("account api", "/api/health"); //map health check api
                
                opt.UseApiEndpointHttpMessageHandler(_ => {
                    return new HttpClientHandler {
                        ClientCertificateOptions = ClientCertificateOption.Manual, ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                    };
                });
                
            })
            .AddInMemoryStorage();
    }
}