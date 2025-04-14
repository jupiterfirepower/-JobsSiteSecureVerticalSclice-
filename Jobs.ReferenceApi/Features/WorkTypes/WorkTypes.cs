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
using Jobs.ReferenceApi.Helpers;
using Jobs.ReferenceApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Jobs.ReferenceApi.Features.WorkTypes;

public static class WorkTypes
{
    public record QueryList : IRequest<List<WorkTypeDto>>;
    
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<WorkTypeDto, WorkType>().ReverseMap();
        }
    }
    
    public class WorkTypeEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/work-types", async Task<Results<Ok<List<WorkTypeDto>>, BadRequest>> (HttpContext context, 
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
            
                    var items = await mediatr.Send(new QueryList());
                    return TypedResults.Ok(items);
                }).WithName("GetWorkTypes")
                .RequireRateLimiting("FixedWindow")
                .WithOpenApi();
        }
    }
    
    public interface IWorkTypeService
    {
        Task<List<WorkTypeDto>> GetWorkTypesAsync();
    }
    
    public class WorkTypeService(IGenericRepository<WorkType> workTypesRepository, 
        ICacheService cacheService, 
        IMapper mapper) : BaseProcessingService(mapper), IWorkTypeService
    {
        //private readonly IMapper _mapper = mapper;

        public async Task<List<WorkTypeDto>> GetWorkTypesAsync()
        {
            await CheckOrLoadWorkTypesData();
        
            return cacheService.GetData<List<WorkTypeDto>>("workTypes");
        }
        
        private async Task LoadWorkTypesDataToLocalCacheService()
        {
            var workTypes = await Task.FromResult(GetDataAsync<WorkType, WorkTypeDto>("./StorageData/WorkTypes.json",async () => await workTypesRepository.GetAllAsync()));
            cacheService.SetData("workTypes", workTypes, DateTimeOffset.UtcNow.AddYears(100));
        }
    
        # region CheckAndLoad
        protected async Task CheckOrLoadWorkTypesData()
        {
            if (!cacheService.HasData("workTypes"))
                await LoadWorkTypesDataToLocalCacheService();
        }
        # endregion

    }

    public class ListWorkTypesQueryHandler(IWorkTypeService service) : IRequestHandler<QueryList, List<WorkTypeDto>>
    {
        public async Task<List<WorkTypeDto>> Handle(QueryList request, CancellationToken cancellationToken) =>
            await service.GetWorkTypesAsync();
    }
}