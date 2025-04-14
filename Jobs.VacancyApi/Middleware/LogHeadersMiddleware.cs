namespace Jobs.VacancyApi.Middleware;

public class LogHeadersMiddleware(RequestDelegate next, ILogger<LogHeadersMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        foreach (var header in context.Request.Headers)
        {
            logger.LogInformation("Header: {Key}: {Value}", header.Key, header.Value);
        }

        await next(context);
    }
}

// Extension method to make it easy to add the middleware to the pipeline
public static class LogHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseLogHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LogHeadersMiddleware>();
    }
}