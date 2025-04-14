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

namespace Jobs.CompanyApi.Features.Companies;

public static class DeleteCompany
{
    public record RequestDeleteCompanyCommand(int Id) : IRequest<int>;
    
    public class CompanyEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/companies/{id:int}", async Task<Results<BadRequest, NoContent, NotFound>> (int id, 
                    ISender mediatr,
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
                    GuardsHelper.Guards(mediatr, service, cryptService, signedNonceService, httpContextAccessor);
        
                    if (ApiSecurityHelper.IsBadRequest(httpContextAccessor, 
                            cryptService, signedNonceService, service, 
                            apiKey, signedNonce, apiSecret))
                    {
                        return TypedResults.BadRequest();
                    }

                    var result = await mediatr.Send(new RequestDeleteCompanyCommand(id));
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
                .WithName("RemoveCompany")
                .RequireRateLimiting("FixedWindow")
                .WithOpenApi();
        }
    }
    
    public interface IDeleteCompanyService
    {
        Task<int> DeleteCompany(int id);
    }
    
    public class DeleteCompanyService(IGenericRepository<Company> repository, IMapper mapper) : IDeleteCompanyService
    {
        public async Task<int> DeleteCompany(int id)
        {
            bool result = repository.Remove(id);
            await repository.SaveAsync();
            return result ? 0 : -1;
        }
    }
    
    public class DeleteCompanyCommandHandler(IDeleteCompanyService service) : IRequestHandler<RequestDeleteCompanyCommand, int>
    {
        public async Task<int> Handle(RequestDeleteCompanyCommand command, CancellationToken cancellationToken) =>
            await service.DeleteCompany(command.Id);
    }
}