using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Jobs.Common.Constants;
using Jobs.Common.Contracts;
using Jobs.CompanyApi.Features.Notifications;
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
using Serilog;

namespace Jobs.CompanyApi.Features.Companies;

public static class CreateCompany
{
    public record RequestCreateCompanyCommand(CompanyInDto Company) : IRequest<CompanyDto>;
    
    public record Results(CompanyDto Data);
    
    public class CreateCompanyEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/companies", async Task<Results<Created<CompanyDto>, BadRequest>> (
                    [FromBody] CompanyInDto company,
                    [FromServices] IApiKeyService service,
                    [FromServices] ISignedNonceService signedNonceService,
                    [FromServices] IEncryptionService cryptService,
                    [FromServices] ISender mediatr,
                    [FromServices] IPublisher publisher,
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
                    Log.Information("CreateCompanyEndpoint method post.");
                        
                    GuardsHelper.Guards(mediatr, service, cryptService, signedNonceService, httpContextAccessor);

                    if (ApiSecurityHelper.IsBadRequest(httpContextAccessor,
                            cryptService, signedNonceService, service,
                            apiKey, signedNonce, apiSecret))
                    {
                        return TypedResults.BadRequest();
                    }

                    var sanitized = SanitizerDtoHelper.SanitizeCompanyInDto(company);

                    var result = await mediatr.Send(new RequestCreateCompanyCommand(sanitized));
                    if (0 == result.CompanyId) return TypedResults.BadRequest();

                    await publisher.Publish(new CompanyCreatedNotification(result.CompanyId));

                    return TypedResults.Created($"/companies/{result.CompanyId}", result);
                })
                .AddEndpointFilter(async (context, next) =>
                {
                    var company = context.GetArgument<CompanyInDto>(0);

                    if (company.CompanyId != 0)
                    {
                        return TypedResults.BadRequest();
                    }

                    return await next(context);
                })
                .AddEndpointFilter<DtoModeValidationFilter<CompanyInDto>>()
                .WithName("AddCompany")
                .RequireRateLimiting("FixedWindow")
                .WithOpenApi();
        }
    }
    
    public interface ICreateCompanyService
    {
        Task<CompanyDto> CreateCompany(CompanyInDto vacancy);
    }
    
    public class CreateCompanyService(IGenericRepository<Company> repository, IMapper mapper) : ICreateCompanyService
    {
        public async Task<CompanyDto> CreateCompany(CompanyInDto vacancy)
        {
            var newCompany = mapper.Map<Company>(vacancy);
            repository.Add(newCompany);
            await repository.SaveAsync();
            return mapper.Map<CompanyDto>(newCompany);
        }
    }
    
    public class CreateCompanyCommandHandler(ICreateCompanyService service) : IRequestHandler<RequestCreateCompanyCommand, CompanyDto>
    {
        public async Task<CompanyDto> Handle(RequestCreateCompanyCommand command, CancellationToken cancellationToken) =>
            await service.CreateCompany(command.Company);
    }
}