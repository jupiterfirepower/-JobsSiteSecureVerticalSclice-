using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Jobs.Common.Constants;
using Jobs.Common.Contracts;
using Jobs.CompanyApi.Helpers;
using Jobs.Core.Contracts;
using Jobs.Core.Filters;
using Jobs.Core.Helpers;
using Jobs.DTO;
using Jobs.DTO.In;
using Jobs.Entities.Models;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Jobs.CompanyApi.Features.Companies;

public static class UpdateCompany
{
    public record RequestUpdateCompanyCommand(CompanyInDto Company) : IRequest<int>;
    
    public record Results(CompanyDto Data);
    
    public class UpdateCompanyEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/companies/{id:int}", async Task<Results<BadRequest, NoContent, NotFound>> (int id, 
                CompanyInDto company, 
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
                GuardsHelper.Guards(mediatr, service, cryptService, signedNonceService, httpContextAccessor);
                
                if (ApiSecurityHelper.IsBadRequest(httpContextAccessor, 
                        cryptService, signedNonceService, service, 
                        apiKey, signedNonce, apiSecret))
                {
                    return TypedResults.BadRequest();
                }

                var sanitized = SanitizerDtoHelper.SanitizeCompanyInDto(company);
                var result = await mediatr.Send(new RequestUpdateCompanyCommand(sanitized));

                return result > 0 ? TypedResults.NoContent() : TypedResults.NotFound();
            })
            .AddEndpointFilter(async (context, next) =>
            {
                var id = context.GetArgument<int>(0);
                var company = context.GetArgument<CompanyInDto>(1);
            
                if (id <= 0 || id != company.CompanyId)
                {
                    return TypedResults.BadRequest();
                }

                return await next(context);
            })
            .AddEndpointFilter<DtoModeValidationFilter<CompanyInDto>>()
            .WithName("ChangeCompany")
            .RequireRateLimiting("FixedWindow")
            .WithOpenApi(); 
        }
    }
    
    public interface IUpdateCompanyService
    {
        Task<int> UpdateCompany(CompanyInDto company);
    }
    
    public class UpdateCompanyService(IGenericRepository<Company> repository, IMapper mapper) : IUpdateCompanyService
    {
        public async Task<int> UpdateCompany(CompanyInDto company)
        {
            var currentCompany = await repository.GetByIdAsync(company.CompanyId);
        
            if (currentCompany == null)
            {
                return -1;
            }

            var current = mapper.Map<Company>(company);
            repository.Change(currentCompany, current);
            await repository.SaveAsync();
            return currentCompany.CompanyId;
        }
    }
    
    public class UpdateCompanyCommandHandler(IUpdateCompanyService service) : IRequestHandler<RequestUpdateCompanyCommand, int>
    {
        public async Task<int> Handle(RequestUpdateCompanyCommand command, CancellationToken cancellationToken) =>
            await service.UpdateCompany(command.Company);
    }
}