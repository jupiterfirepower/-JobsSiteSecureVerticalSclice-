using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using AutoMapper;
using Jobs.Common.Constants;
using Jobs.Common.Contracts;
using Jobs.Common.Extentions;
using Jobs.Core.Contracts;
using Jobs.Core.Extentions;
using Jobs.Core.Helpers;
using Jobs.DTO;
using Jobs.Entities.Models;
using Jobs.ReferenceApi.Contracts;
using Jobs.ReferenceApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Jobs.ReferenceApi.Features.Categories;

public static class Categories
{
    public record QueryList : IRequest<List<CategoryDto>>;

    public record Result(List<CategoryDto> Data);

    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CategoryDto, Category>().ReverseMap();
        }
    }
    
    public class CategoriesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            /*var version1 = new ApiVersion(1);

            var apiVersionSet = app.NewApiVersionSet()
                .HasApiVersion(version1)
                .ReportApiVersions()
                .Build();*/
            //api/v{version:apiVersion}
            app.MapGet("/categories", async Task<Results<Ok<List<CategoryDto>>, BadRequest>> (HttpContext context, 
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
                    Console.WriteLine("Start Get Categories.");
           
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
                }).WithName("GetCategories")
                //.MapApiVersion(apiVersionSet, version1)
                .RequireRateLimiting("FixedWindow")
                .WithOpenApi();
        }
    }
    
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetCategoriesAsync();
    }
    
    public class CategoryService(IGenericRepository<Category> categoryRepository, 
        ICacheService cacheService, 
        IMapper mapper) : BaseProcessingService(mapper), ICategoryService
    {
        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            await CheckOrLoadCategoriesData();
            
            return cacheService.GetData<List<CategoryDto>>("categories");
        }
        
        private async Task LoadCategoriesDataToLocalCacheService()
        {
            var categories = await Task.FromResult(GetDataAsync<Category, CategoryDto>("./StorageData/Categories.json",async () => await categoryRepository.GetAllAsync()));
            cacheService.SetData("categories", categories, DateTimeOffset.UtcNow.AddYears(100));
        }
    
        # region CheckAndLoad
        private async Task CheckOrLoadCategoriesData()
        {
            if (!cacheService.HasData("categories"))
                await LoadCategoriesDataToLocalCacheService();
        }
        # endregion

    }
    
    public class ListCategoriesQueryHandler(ICategoryService service) : IRequestHandler<QueryList, List<CategoryDto>>
    {
        public async Task<List<CategoryDto>> Handle(QueryList request, CancellationToken cancellationToken) =>
            await service.GetCategoriesAsync();
    }

}