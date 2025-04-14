using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AutoMapper;
using Jobs.AccountApi.Contracts;
using Jobs.Common.Constants;
using Jobs.Common.Extentions;
using Jobs.Common.Responses;
using Jobs.Core.Contracts;
using Jobs.Core.Helpers;
using Jobs.Dto.Request;
using Jobs.Entities.DataModel;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Jobs.AccountApi.Features.Keycloak;

public static class RefreshToken
{
    public record RequestRefreshTokenCommand(string RefreshToken) : IRequest<KeycloakRespone?>;
    
    public record Results(RegisterUserResponse Result);
    
    public class RefreshKeycloakTokenProfile : Profile
    {
        public RefreshKeycloakTokenProfile()
        {
           // CreateMap<UserDto , User>().ReverseMap();
        }
    }
    
    public class RefreshTokenCommandEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/refresh", async Task<Results<Ok<KeycloakRespone>, BadRequest>> ([FromBody] string refreshToken,
                HttpContext context,
                [FromServices] IApiKeyService service,
                [FromServices] IKeycloakAccountService accountService,
                [FromServices] ISender mediatr,
                [FromServices] IPublisher publisher,
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
                Log.Information($"Get token by refresh token!");
                Console.WriteLine($"UserAgent - {httpContextAccessor.HttpContext?.Request.Headers.UserAgent}");
                Console.WriteLine($"RefreshToken - {refreshToken}");
                
                GuardsHelper.Guards(mediatr, service, cryptService, signedNonceService, httpContextAccessor);

                if (ApiSecurityHelper.IsBadRequest(httpContextAccessor,
                        cryptService, signedNonceService, service,
                        apiKey, signedNonce, apiSecret))
                {
                    return TypedResults.BadRequest();
                }

                var ipAddress = context.Request.GetIpAddress();
                Log.Information($"ClientIPAddress - {ipAddress}.");

                //var result = await accountService.RefreshTokenAsync(refreshToken).ConfigureAwait(false);
                var result = await mediatr.Send(new RequestRefreshTokenCommand(refreshToken));

                return TypedResults.Ok(result);
            })
            .WithName("RefreshToken")
            .WithOpenApi()
            .AllowAnonymous()
            .RequireRateLimiting("FixedWindow");
        }
    }
    
    public interface IKeycloakRefreshTokenService
    {
        Task<KeycloakRespone?> RefreshTokenAsync(string refreshToken);
    }
    
    public class KeycloakRefreshTokenService(IHttpClientFactory httpClientFactory, IMapper mapper,
        string baseUrl = "http://localhost:9001", string realm = "mjobs", 
        string adminUserName = "admin", string adminPassword = "newpwd") : IKeycloakRefreshTokenService
    {
        public async Task<KeycloakRespone?> RefreshTokenAsync(string refreshToken)
        {
            using var httpClient = httpClientFactory.CreateClient();

            // Form data is typically sent as key-value pairs
            var adminData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "refresh_token"),
                new("client_id", "admin-cli"),
                new("refresh_token", refreshToken)
            };

            // Encodes the key-value pairs for the ContentType 'application/x-www-form-urlencoded'
            using var adminContent = new FormUrlEncodedContent(adminData);

            var adminResponse =
                await httpClient.PostAsync($"{baseUrl.TrimEnd('/')}/realms/master/protocol/openid-connect/token",
                    adminContent);
            adminResponse.EnsureSuccessStatusCode();

            var result = await adminResponse.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<KeycloakRespone>(result);
            return data;
        }
    }
    
    public class KeycloakRefreshTokenCommandHandler(IKeycloakRefreshTokenService service) : IRequestHandler<RequestRefreshTokenCommand, KeycloakRespone?>
    {
        public async Task<KeycloakRespone?> Handle(RequestRefreshTokenCommand command, CancellationToken cancellationToken) =>
            await service.RefreshTokenAsync(command.RefreshToken);
    }
}