using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;

namespace Jobs.Core.Extentions;

public static class ApiVersionBuilderExtension
{
    public static IEndpointConventionBuilder MapApiVersion(
        this IEndpointConventionBuilder endpoints,
        ApiVersionSet apiVersionSet,
        ApiVersion apiVersion)

    {
        if (endpoints == null)
        {
            throw new ArgumentNullException(nameof(endpoints));
        }

        return endpoints.WithApiVersionSet(apiVersionSet)
            .HasApiVersion(apiVersion)
            .MapToApiVersion(apiVersion);
    }
}