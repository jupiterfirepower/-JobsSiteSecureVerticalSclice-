using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace Jobs.Core.Extentions;

public static class RateLimiterBuilderExtention
{
    public static IServiceCollection AddRateLimiterService(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(), 
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));
    
            options.OnRejected = (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString(CultureInfo.InvariantCulture);
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);

                return new ValueTask();
            };
        });
        
        return services;
    }
    
    public static IServiceCollection AddWindowRateLimiterService(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("FixedWindow", opt => {
                opt.Window = TimeSpan.FromSeconds(10);
                opt.PermitLimit = 100;
            });
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            
        });
        /*
        services.AddRateLimiter(_ => _
            .AddConcurrencyLimiter(policyName: "ConcurrencyRateLimiter", options =>
            {
                // A max of 10 requests at the same time are allowed
                options.PermitLimit = 10;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 4;
            }));*/
        
        return services;
    }
}