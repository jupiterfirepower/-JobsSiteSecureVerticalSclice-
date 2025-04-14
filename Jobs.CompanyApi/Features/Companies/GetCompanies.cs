using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AutoMapper;
using Jobs.Common.Constants;
using Jobs.Common.Contracts;
using Jobs.Common.Extentions;
using Jobs.Core.Contracts;
using Jobs.Core.Extentions;
using Jobs.Core.Helpers;
using Jobs.DTO;
using Jobs.Entities.Models;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Core;

namespace Jobs.CompanyApi.Features.Companies;

public static class GetCompanies
{
    public record RequestListCompaniesQuery : IRequest<List<CompanyDto>>;

    public record Results(List<CompanyDto> Data);
    
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CompanyDto, Company>();

            CreateMap<Company, CompanyDto>()
                .ConstructUsing(x => new CompanyDto(x.CompanyId, x.CompanyName, x.CompanyDescription, x.CompanyLogoPath, x.CompanyLink, x.IsVisible, x.IsActive));
        }
    }
    
    public class CompaniesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/companies", async Task<Results<Ok<List<CompanyDto>>, BadRequest>> (HttpContext context, 
                    ClaimsPrincipal user,
                    [FromServices] ISender mediatr, 
                    [FromServices] IApiKeyService service,
                    [FromServices] ISignedNonceService signedNonceService,
                    [FromServices] IEncryptionService cryptService,
                    [FromServices] IHttpContextAccessor httpContextAccessor,
                    [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey), Required, 
                     StringLength(HttpHeaderKeys.XApiHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiHeaderKeyMinLength)] string apiKey,
                    [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey), Required, 
                     StringLength(HttpHeaderKeys.SNonceHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.SNonceHeaderKeyMinLength)] string signedNonce,
                    [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey), Required, 
                     StringLength(HttpHeaderKeys.XApiSecretHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiSecretHeaderKeyMinLength)] string apiSecret) =>
                {
                    //LogInformation($"UserName: {user.Identity?.Name}");
                    Console.WriteLine($"UserAgent - {httpContextAccessor.HttpContext?.Request.Headers.UserAgent}");
        
                    GuardsHelper.Guards(mediatr, service, cryptService, signedNonceService, httpContextAccessor);
        
                    if (ApiSecurityHelper.IsBadRequest(httpContextAccessor, 
                            cryptService, signedNonceService, service, 
                            apiKey, signedNonce, apiSecret))
                    {
                        return TypedResults.BadRequest();
                    }

                    var ipAddress = context.Request.GetIpAddress();
                    Log.Information($"ClientIPAddress - {ipAddress}.");
        
                    var companies = await mediatr.Send(new RequestListCompaniesQuery());
                    return TypedResults.Ok(companies);
                }).WithName("GetCompanies")
                .RequireRateLimiting("FixedWindow")
                .RequireAuthorization()
                .WithOpenApi();
        }
    }
    
    public interface ICompaniesService
    {
        Task<List<CompanyDto>> GetCompanies();
    }
    
    public class CompaniesService(IGenericRepository<Company> repository, IMapper mapper) : ICompaniesService
    {
        public async Task<List<CompanyDto>> GetCompanies()
        {
            var vacancies = await repository.GetAllAsync();
            return mapper.Map<List<CompanyDto>>(vacancies);
        }
    }
    
    public class ListCompaniesQueryHandler(ICompaniesService service) : IRequestHandler<RequestListCompaniesQuery, List<CompanyDto>>
    {
        public async Task<List<CompanyDto>> Handle(RequestListCompaniesQuery request, CancellationToken cancellationToken) => await service.GetCompanies();
    }
}