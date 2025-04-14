using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Jobs.Common.Constants;
using Jobs.Common.Contracts;
using Jobs.Common.Extentions;
using Jobs.Core.Contracts;
using Jobs.Core.Helpers;
using Jobs.DTO;
using Jobs.Entities.Models;
using Jobs.VacancyApi.Data;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Jobs.VacancyApi.Features.Vacancies;

public static class GetVacancy
{
    public record RequestGetVacancyQuery(int Id): IRequest<VacancyDto>;
    
    public record Result(VacancyDto Data);
    
    public class VacancyEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/vacancies/{id:int}", async Task<Results<Ok<VacancyDto>, BadRequest, NotFound>> (int id, 
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

            var vacancy = await mediatr.Send(new RequestGetVacancyQuery(id));
            return vacancy == null ? TypedResults.NotFound() : TypedResults.Ok(vacancy);
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
        .WithName("GetVacancy")
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();
        }
    }
    
    public interface IVacancyService
    {
        Task<VacancyDto> GetVacancyById(int id);
    }
    
    public class GetVacancyService(IGenericRepository<Vacancy> repository, IMapper mapper) :  IVacancyService
    {
        public async Task<VacancyDto> GetVacancyById(int id)
        {
            var vacancy = await repository.GetByIdAsync(id);
            return mapper.Map<VacancyDto>(vacancy);
        }
    }
    
    public class GetVacancyQueryHandler(IVacancyService service) : IRequestHandler<RequestGetVacancyQuery, VacancyDto>
    {
        public async Task<VacancyDto> Handle(RequestGetVacancyQuery request, CancellationToken cancellationToken) => await service.GetVacancyById(request.Id);
    }
}