using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Jobs.Common.Constants;
using Jobs.Common.Contracts;
using Jobs.Common.Extentions;
using Jobs.Core.Contracts;
using Jobs.Core.Helpers;
using Jobs.DTO;
using Jobs.Entities.Models;
using Jobs.ReferenceApi.Contracts;
using Jobs.ReferenceApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Jobs.ReferenceApi.Features.EmploymentTypes;

public static class GetEmploymentTypeById
{
    public record GetQuery(int Id): IRequest<EmploymentTypeDto>;
    
    private static readonly ConcurrentDictionary<int, EmploymentTypeDto> Cache = new ();

    private static Func<int, EmploymentTypeDto> Memoize(this Func<int, EmploymentTypeDto> f)
    {
        return a => Cache.GetOrAdd(a, f);
    }
    
    public class GetEmploymentTypeEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            //api/v{version:apiVersion}
            app.MapGet("/emp-types/{id:int}", async Task<Results<Ok<EmploymentTypeDto>, BadRequest, NotFound>> (int id, 
                HttpContext context, 
                [FromServices] ISender mediatr, 
                [FromServices] IApiKeyService service, 
                [FromServices] IEncryptionService cryptService,
                [FromServices] ISignedNonceService signedNonceService,
                [FromServices] IHttpContextAccessor httpContextAccessor,
                [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey), Required, 
                 StringLength(HttpHeaderKeys.XApiHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiHeaderKeyMinLength)] string apiKey,
                [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey), Required, 
                 StringLength(HttpHeaderKeys.SNonceHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.SNonceHeaderKeyMinLength)] string signedNonce,
                [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey), Required, 
                 StringLength(HttpHeaderKeys.XApiSecretHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiSecretHeaderKeyMinLength)] string apiSecret) =>
            {
                GuardsHelper.Guards(mediatr, service, cryptService, signedNonceService, httpContextAccessor);
                
                if (ApiSecurityHelper.IsBadRequest(httpContextAccessor, 
                        cryptService, signedNonceService, service, 
                        apiKey, signedNonce, apiSecret))
                {
                    return TypedResults.BadRequest();
                }
                
                var ipAddress = context.Request.GetIpAddress();
                Log.Information($"ClientIPAddress - {ipAddress}.");
                
                Func<int, EmploymentTypeDto> getEmploymentType = xid =>
                {
                    Console.WriteLine($"GetEmploymentTypeById - {xid}.");
                    var task = Task.Run(async () => await mediatr.Send(new GetQuery(xid)));
                    return task.ConfigureAwait(false).GetAwaiter().GetResult();
                };

                var memoizedFunc = getEmploymentType.Memoize();
                var result = memoizedFunc(id);
                
                //var result = await mediatr.Send(new GetQuery(id));
                return result != null ? TypedResults.Ok(result) : TypedResults.NotFound();
            })
            .AddEndpointFilter(async (context, next) =>
            {
                var id = context.GetArgument<int>(0);
       
                if (id <= 0)
                {
                    return TypedResults.BadRequest();
                }

                return await next(context);
            })
            .WithName("GetEmploymentType")
            .RequireRateLimiting("FixedWindow")
            .WithOpenApi();
        }
    }
    public interface IEmploymentTypeServiceExtended: EmploymentTypes.IEmploymentTypeService
    {
        Task<EmploymentTypeDto> GetEmploymentTypeByIdAsync(int id);
    }
    
    public class EmploymentTypeServiceExtended(IGenericRepository<EmploymentType> empRepository, 
        ICacheService cacheService, 
        IMapper mapper) : EmploymentTypes.EmploymentTypeService(empRepository, cacheService, mapper), IEmploymentTypeServiceExtended
    {
        private readonly ICacheService _cacheService = cacheService;
        private readonly IMapper _mapper1 = mapper;

        public async Task<EmploymentTypeDto> GetEmploymentTypeByIdAsync(int id)
        {
            await CheckOrLoadEmploymentTypesData();
        
            var currentListData = _cacheService.GetData<List<EmploymentTypeDto>>("empTypes");
            var current = currentListData.FirstOrDefault(x=>x.EmploymentTypeId == id);

            return _mapper1.Map<EmploymentTypeDto>(current);
        }
    }
    
    public class GetEmploymentTypeQueryHandler(IEmploymentTypeServiceExtended service) : IRequestHandler<GetQuery, EmploymentTypeDto>
    {
        public async Task<EmploymentTypeDto> Handle(GetQuery request, CancellationToken cancellationToken) => 
            await service.GetEmploymentTypeByIdAsync(request.Id);
    }
}