using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Jobs.Common.Constants;
using Jobs.Common.Contracts;
using Jobs.Core.Contracts;
using Jobs.Core.Helpers;
using Jobs.DTO;
using Jobs.Entities.Models;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Jobs.VacancyApi.Features.Vacancies;

public static class DeleteVacancy
{
    public record RequestDeleteVacancyCommand(int Id) : IRequest<int>;

    public record Result(bool Success);
    
    public class DeleteVacancyEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/vacancies/{id:int}",  async Task<Results<BadRequest, NotFound, NoContent>> (int id, 
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

                var result = await mediatr.Send(new RequestDeleteVacancyCommand(id));
                return result == -1 ? TypedResults.NotFound() : TypedResults.NoContent();
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
            .WithName("RemoveVacancy")
            .RequireRateLimiting("FixedWindow")
            .WithOpenApi();
            }
    }
    
    public interface IDeleteVacancyService
    {
        Task<int> DeleteVacancy(int id);
    }
    
    public class DeleteVacancyService(IGenericRepository<Vacancy> repository) :  IDeleteVacancyService
    {
        public async Task<int> DeleteVacancy(int id)
        {
            bool result = repository.Remove(id);
            await repository.SaveAsync();
            return result ? 0 : -1;
        }
    }
    
    public class DeleteVacancyCommandHandler(IDeleteVacancyService service) : IRequestHandler<RequestDeleteVacancyCommand, int>
    {
        public async Task<int> Handle(RequestDeleteVacancyCommand command, CancellationToken cancellationToken) =>
            await service.DeleteVacancy(command.Id);
    }
}