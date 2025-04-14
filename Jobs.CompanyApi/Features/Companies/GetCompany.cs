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

public static class GetCompany
{
    public record RequestGetCompanyQuery(int Id): IRequest<CompanyDto>;
    
    public record Results(CompanyDto Data);
    
    public class CompanyEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/companies/{id:int}", async Task<Results<Ok<CompanyDto>, BadRequest, NotFound>> (int id,
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

                    var company = await mediatr.Send(new RequestGetCompanyQuery(id));
                    return company == null ? TypedResults.NotFound() : TypedResults.Ok(company);
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
                .WithName("GetCompany")
                .RequireRateLimiting("FixedWindow")
                .WithOpenApi();
        }
    }
    
    public interface ICompanyService
    {
        Task<CompanyDto> GetCompanyById(int id);
    }
    
    public class CompanyService(IGenericRepository<Company> repository, IMapper mapper) : ICompanyService
    {
        public async Task<CompanyDto> GetCompanyById(int id)
        {
            var company = await repository.GetByIdAsync(id);
            return mapper.Map<CompanyDto>(company);
        }
    }
    
    public class GetCompanyQueryHandler(ICompanyService service) : IRequestHandler<RequestGetCompanyQuery, CompanyDto>
    {
        public async Task<CompanyDto> Handle(RequestGetCompanyQuery request, CancellationToken cancellationToken) => await service.GetCompanyById(request.Id);
    }
}