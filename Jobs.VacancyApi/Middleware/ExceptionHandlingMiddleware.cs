using System.Net;

namespace Jobs.VacancyApi.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine("An exception occurred: " + ex.Message);
 
            // Set the response status code to 500 Internal Server Error
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }
}

// Extension method to make it easy to add the middleware to the pipeline
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlers(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}