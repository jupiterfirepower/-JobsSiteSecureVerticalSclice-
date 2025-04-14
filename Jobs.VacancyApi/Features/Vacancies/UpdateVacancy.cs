using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Jobs.Common.Constants;
using Jobs.Common.Contracts;
using Jobs.Core.Contracts;
using Jobs.Core.Filters;
using Jobs.Core.Helpers;
using Jobs.DTO;
using Jobs.DTO.In;
using Jobs.Entities.Models;
using Jobs.VacancyApi.Data;
using Jobs.VacancyApi.Features.Vacancies.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jobs.VacancyApi.Features.Vacancies;

public static class UpdateVacancy
{
    public record RequestUpdateVacancyCommand(VacancyInDto Vacancy) : IRequest<int>;
    
    public record Result(List<VacancyDto> Data);

    public class VacancyEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/vacancies/{id:int}",
                        async Task<Results<BadRequest, NotFound, NoContent>> (int id,
                        [FromBody] VacancyInDto vacancy,
                        [FromServices] ISender mediatr,
                        [FromServices] IApiKeyService service,
                        [FromServices] IEncryptionService cryptService,
                        [FromServices] ISignedNonceService signedNonceService,
                        [FromServices] IHttpContextAccessor httpContextAccessor,
                        [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey), Required, StringLength(HttpHeaderKeys.XApiHeaderKeyMaxLength,
                             MinimumLength = HttpHeaderKeys.XApiHeaderKeyMinLength)]
                        string apiKey,
                        [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey), Required, StringLength(HttpHeaderKeys.SNonceHeaderKeyMaxLength,
                             MinimumLength = HttpHeaderKeys.SNonceHeaderKeyMinLength)]
                        string signedNonce,
                        [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey), Required, StringLength(HttpHeaderKeys.XApiSecretHeaderKeyMaxLength,
                             MinimumLength = HttpHeaderKeys.XApiSecretHeaderKeyMinLength)]
                        string apiSecret) =>
                    {
                        GuardsHelper.Guards(mediatr, service, cryptService, signedNonceService, httpContextAccessor);

                        if (ApiSecurityHelper.IsBadRequest(httpContextAccessor, cryptService,
                                signedNonceService, service, apiKey, signedNonce, apiSecret))
                        {
                            return TypedResults.BadRequest();
                        }

                        var sanitized = SanitizeHelper.SanitizeVacancyInDto(vacancy);

                        var result = await mediatr.Send(new RequestUpdateVacancyCommand(sanitized));

                        return result > 0 ? TypedResults.NoContent() : TypedResults.NotFound();
                    })
                .AddEndpointFilter(async (context, next) =>
                {
                    var id = context.GetArgument<int>(0);
                    var vacancy = context.GetArgument<VacancyInDto>(1);

                    if (id <= 0 || id != vacancy.VacancyId)
                    {
                        return TypedResults.BadRequest();
                    }

                    return await next(context);
                })
                .AddEndpointFilter<DtoModeValidationFilter<VacancyInDto>>()
                .WithName("ChangeVacancy")
                .RequireRateLimiting("FixedWindow")
                .WithOpenApi();
        }
    }
    
    public interface IUpdateVacancyService
    {
        Task<int> UpdateVacancy(VacancyInDto vacancy);
    }
    
    public class UpdateVacancyService(IGenericRepository<Vacancy> repository, IMapper mapper) :  IUpdateVacancyService
    {
        public async Task<int> UpdateVacancy(VacancyInDto vacancy)
        {
            var currentVacancy = await repository.GetByIdAsync(vacancy.VacancyId);
        
            if (currentVacancy == null)
            {
                return -1;
            }

            var current = mapper.Map<Vacancy>(vacancy);
            repository.Change(currentVacancy, current);
            await repository.SaveAsync();
            return currentVacancy.VacancyId;
        }
    }
    
    public class UpdateVacancyCommandHandler(IUpdateVacancyService service) : IRequestHandler<RequestUpdateVacancyCommand, int>
    {
        public async Task<int> Handle(RequestUpdateVacancyCommand command, CancellationToken cancellationToken) =>
            await service.UpdateVacancy(command.Vacancy);
    }

}