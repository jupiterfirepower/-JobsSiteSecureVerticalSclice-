using System.ComponentModel.DataAnnotations;
using Jobs.Common.Constants;
using Jobs.Core.Contracts;
using Jobs.Core.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Jobs.CompanyApi.Features;

public static class GetApiKeyToken
{
    public class GetApiKeyTokenEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/token", async Task<Results<Ok<bool>, BadRequest>> (
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
                
                    if (ApiSecurityHelper.IsTrustBadRequest(httpContextAccessor, 
                            cryptService, signedNonceService, service, 
                            apiKey, signedNonce, apiSecret))
                    {
                        return TypedResults.BadRequest();
                    }

                    var result = await Task.FromResult(true);
                    return TypedResults.Ok(result);
                })
                .WithName("GetToken")
                .RequireRateLimiting("FixedWindow")
                .WithOpenApi();
        }
    }
}