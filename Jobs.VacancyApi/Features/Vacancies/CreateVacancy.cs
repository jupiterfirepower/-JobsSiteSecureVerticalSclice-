using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Jobs.Common.Constants;
using Jobs.Common.Contracts;
using Jobs.Core.Contracts;
using Jobs.Core.Extentions;
using Jobs.Core.Filters;
using Jobs.Core.Helpers;
using Jobs.DTO;
using Jobs.DTO.In;
using Jobs.Entities.Models;
using Jobs.VacancyApi.Data;
using Jobs.VacancyApi.Features.Notifications;
using Jobs.VacancyApi.Features.Vacancies.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jobs.VacancyApi.Features.Vacancies;

public static class CreateVacancy
{
    public record RequestCreateVacancyCommand(VacancyInDto Vacancy) : IRequest<VacancyDto>;
    
    public record Result(VacancyDto Data);
    
    public class CreateVacancyProfile : Profile
    {
        public CreateVacancyProfile()
        {
            CreateMap<VacancyInDto, Vacancy>()
                .ForMember(x => x.Category, opt => opt.Ignore());
        }
    }

    public class CreateVacancyEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/vacancies", async Task<Results<Created<VacancyDto>, BadRequest>> (
                    VacancyInDto vacancy,
                    [FromServices] IApiKeyService service,
                    [FromServices] IEncryptionService cryptService,
                    [FromServices] ISender mediatr,
                    [FromServices] IPublisher publisher,
                    [FromServices] ISignedNonceService signedNonceService,
                    [FromServices] IHttpContextAccessor httpContextAccessor,
                    [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey), Required,
                     StringLength(HttpHeaderKeys.XApiHeaderKeyMaxLength,
                         MinimumLength = HttpHeaderKeys.XApiHeaderKeyMinLength)]
                    string apiKey,
                    [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey), Required,
                     StringLength(HttpHeaderKeys.SNonceHeaderKeyMaxLength,
                         MinimumLength = HttpHeaderKeys.SNonceHeaderKeyMinLength)]
                    string signedNonce,
                    [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey), Required,
                     StringLength(HttpHeaderKeys.XApiSecretHeaderKeyMaxLength,
                         MinimumLength = HttpHeaderKeys.XApiSecretHeaderKeyMinLength)]
                    string apiSecret) =>
                {
                    GuardsHelper.Guards(mediatr, service, cryptService, signedNonceService, httpContextAccessor);

                    if (ApiSecurityHelper.IsBadRequest(httpContextAccessor,
                            cryptService, signedNonceService, service,
                            apiKey, signedNonce, apiSecret))
                    {
                        return TypedResults.BadRequest();
                    }

                    var sanitized = SanitizeHelper.SanitizeVacancyInDto(vacancy);

                    var result = await mediatr.Send(new RequestCreateVacancyCommand(sanitized));

                    if (0 == result.VacancyId) return TypedResults.BadRequest();

                    await publisher.Publish(new VacancyCreatedNotification(result.VacancyId));

                    return TypedResults.Created($"/vacancies/{result.VacancyId}", result);
                })
                .AddEndpointFilter(async (context, next) =>
                {
                    var vacancy = context.GetArgument<VacancyInDto>(0);

                    if (vacancy.VacancyId != 0)
                    {
                        return TypedResults.BadRequest();
                    }

                    return await next(context);
                })
                .AddEndpointFilter<DtoModeValidationFilter<VacancyInDto>>()
                .WithName("AddVacancy")
                .RequireRateLimiting("FixedWindow")
                .WithOpenApi();
        }
    }
    
    public interface ICreateVacancyService
    {
        Task<VacancyDto> CreateVacancy(VacancyInDto vacancy);
    }
    
    public class CreateVacancyService(IGenericRepository<Vacancy> repository, 
        JobsDbContext context, IMapper mapper) :  ICreateVacancyService
    {
        public async Task<VacancyDto> CreateVacancy(VacancyInDto vacancy)
        {
            var newVacancy = mapper.Map<Vacancy>(vacancy);
            repository.Add(newVacancy);
            await repository.SaveAsync();
        
            var paramWorkTypes = string.Join(",", vacancy.WorkTypes);
            var count = await context.Database.ExecuteSqlRawAsync("call sp_save_vac_worktypes(@p0, @p1);", 
                parameters: new[] { (object)newVacancy.VacancyId,  paramWorkTypes});
            Console.WriteLine($"call sp_save_vac_worktypes - {count}");
        
            var paramEmploymentTypes = string.Join(",", vacancy.EmploymentTypes);
            count = await context.Database.ExecuteSqlRawAsync("call sp_save_vac_emptypes(@p0, @p1);", 
                parameters: [newVacancy.VacancyId,  paramEmploymentTypes]);
            Console.WriteLine($"sp_save_vac_emptypes - {count}");
        
            return mapper.Map<VacancyDto>(newVacancy);
        }
    }
    
    public class CreateVacancyCommandHandler(ICreateVacancyService service) : IRequestHandler<RequestCreateVacancyCommand, VacancyDto>
    {
        public async Task<VacancyDto> Handle(RequestCreateVacancyCommand command, CancellationToken cancellationToken) =>
            await service.CreateVacancy(command.Vacancy);
    }

}