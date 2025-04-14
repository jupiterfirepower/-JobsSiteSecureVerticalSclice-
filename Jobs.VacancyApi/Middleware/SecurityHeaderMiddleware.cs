using System.Net;

namespace Jobs.VacancyApi.Middleware;

public class SecurityHeaderMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        //using var scope = app.Services.CreateScope();
        //var service = scope.ServiceProvider.GetRequiredService<IApiKeyService>();
        
        string keyHeader = context.Request.Headers["x-api-key"];
        if (keyHeader is "key")
        {
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
 
        await next(context);
    }
}

// Extension method to make it easy to add the middleware to the pipeline
public static class SecurityHeaderMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeaderMiddleware>();
    }
}