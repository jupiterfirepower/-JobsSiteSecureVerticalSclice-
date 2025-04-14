using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Jobs.Common.Constants;
using Jobs.Common.Contracts;
using Jobs.Common.Extentions;
using Jobs.Core.Contracts;
using Jobs.Core.Helpers;
using Jobs.DTO;
using Jobs.DTO.In;
using Jobs.Entities.Models;
using Jobs.VacancyApi.Contracts;
using Jobs.VacancyApi.Data;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Jobs.VacancyApi.Features.Vacancies;

public static class GetVacancies
{
    public record  RequestListQuery : IRequest<List<VacancyDto>>;

    public record Result(List<VacancyDto> Data);
    
    public class GetVacanciesProfile : Profile
    {
        public GetVacanciesProfile()
        {
            CreateMap<VacancyDto, Vacancy>()
                .ForMember(x => x.Category, opt => opt.Ignore());
        
            CreateMap<Vacancy, VacancyDto>()
                .ConstructUsing(x => new VacancyDto(x.VacancyId, 
                    x.CompanyId, x.CategoryId, 
                    x.VacancyTitle, x.VacancyDescription, 
                    x.SalaryFrom, x.SalaryTo,
                    x.IsVisible, x.IsActive, 
                    x.Created, x.Modified));
        }
    }
    
    public class VacanciesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/vacancies", async Task<Results<Ok<List<VacancyDto>>, BadRequest>> (HttpContext context, 
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
            
                    var products = await mediatr.Send(new RequestListQuery());
                    return TypedResults.Ok(products);
                }).WithName("GetVacancies")
                .RequireRateLimiting("FixedWindow")
                .WithOpenApi();
        }
    }

    public interface IVacanciesService
    {
        Task<List<VacancyDto>> GetVacancies();
    }
    
    public class VacanciesService(IGenericRepository<Vacancy> repository, IMapper mapper) :  IVacanciesService
    {
        public async Task<List<VacancyDto>> GetVacancies()
        {
            var vacancies = await repository.GetAllAsync();
            return mapper.Map<List<VacancyDto>>(vacancies);
        }
    }
    
    public class ListVacanciesQueryHandler(IVacanciesService service) : IRequestHandler<RequestListQuery, List<VacancyDto>>
    {
        public async Task<List<VacancyDto>> Handle(RequestListQuery request, CancellationToken cancellationToken) => await service.GetVacancies();
    }
}