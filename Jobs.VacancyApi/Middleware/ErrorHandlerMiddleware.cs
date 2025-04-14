namespace Jobs.VacancyApi.Middleware;

public class ErrorHandlerMiddleware(ILogger<ErrorHandlerMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            const string message = "An unhandled exception has occurred while executing the request.";
            logger.LogError(exception, message);
            
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}

// Extension method to make it easy to add the middleware to the pipeline
public static class ErrorHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlerMiddleware>();
    }
}