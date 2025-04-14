using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Jobs.CompanyApi.Extentions;

public static class AddHealthChecksExtention
{
    //public static void AddHealthChecks(this IApplicationBuilder app, IEndpointRouteBuilder routes)
    public static void AddHealthChecks(this IEndpointRouteBuilder app)
    {
        // HealthCheck Middleware
        app.MapHealthChecks("/api/health", new HealthCheckOptions
        {
            AllowCachingResponses = false,
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    
        /*app.UseHealthChecksUI(delegate (Options options)
        {
            options.UIPath = "/healthcheck-ui";
        });

        app.MapHealthChecksUI();*/
        
        app.MapHealthChecksUI(setup =>
        {
            setup.UIPath = "/healthcheck-ui";
        });
    }
}