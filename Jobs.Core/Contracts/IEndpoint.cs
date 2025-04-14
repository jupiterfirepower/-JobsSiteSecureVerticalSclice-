using Microsoft.AspNetCore.Routing;

namespace Jobs.Core.Contracts;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}