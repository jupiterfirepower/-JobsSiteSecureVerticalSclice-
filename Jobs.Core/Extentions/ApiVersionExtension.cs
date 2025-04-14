using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Jobs.Core.Extentions;

public static class ApiVersionExtension
{
    public static IServiceCollection AddApiVersionService(this IServiceCollection services)
    {
        services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new(1,0);
            config.ReportApiVersions = true;
            //config.AssumeDefaultVersionWhenUnspecified = false;
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ApiVersionReader = new UrlSegmentApiVersionReader();
            /*config.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("api-x-version")
            );*/
        }).AddApiExplorer(config =>
        {
            //config.GroupNameFormat = "v'V'";
            config.GroupNameFormat = "'v'VVV";
            //config.GroupNameFormat = "'v'V";
            config.SubstituteApiVersionInUrl = true;
        }).EnableApiVersionBinding();
        return services;
    }
}