using Jobs.Core.CustomHealthChecks;
using Jobs.VacancyApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Jobs.VacancyApi.Extentions;

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
            .AddNpgSql(connectionString!,
                name: "Postgres", failureStatus: HealthStatus.Unhealthy, tags: ["Vacancy", "Database"])
            .AddDbContextCheck<JobsDbContext>(
                "Categories check",
                customTestQuery: (db, token) => db.Categories.AnyAsync(token),
                tags: ["ef-db"])
            .AddCheck<MemoryHealthCheck>($"Vacancy Service Memory Check", failureStatus: HealthStatus.Unhealthy, tags:
                ["Vacancy Service"])
            .AddCheck<VaultHealthCheck>("Vault Check", failureStatus: HealthStatus.Unhealthy, tags:
                ["Hashicorp Vault"])
            .AddConsul(option =>
            {
                option.HostName = consulHost!;
                option.Port = consulPort;
                option.RequireHttps = consulRequireHttps;
            }, tags: ["Consul"]);
            //.AddUrlGroup(new Uri("https://localhost:7111/api/v1/heartbeats/ping"), name: "base URL", failureStatus: HealthStatus.Unhealthy);
            
       var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
       Console.WriteLine($"ConfigureHealthChecks Urls: {urls}");
    
        services.AddHealthChecksUI(opt => {
                opt.SetEvaluationTimeInSeconds(60); //time in seconds between check    
                opt.MaximumHistoryEntriesPerEndpoint(30); //maximum history of checks    
                opt.SetApiMaxActiveRequests(1); //api requests concurrency    
                //opt.AddHealthCheckEndpoint("vacancy api", "/api/health"); //map health check api    $"{endpoint}/api/health"
                
                // For Development
                // vacancy-api-1 not needed that code
                // ToDo remove this code.
                opt.UseApiEndpointHttpMessageHandler(_ => {
                    return new HttpClientHandler {
                        ClientCertificateOptions = ClientCertificateOption.Manual, ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                    };
                });
            })
            .AddInMemoryStorage();
    }
}