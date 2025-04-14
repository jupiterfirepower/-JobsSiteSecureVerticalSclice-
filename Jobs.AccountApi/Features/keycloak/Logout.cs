using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using AutoMapper;
using Jobs.AccountApi.Contracts;
using Jobs.AccountApi.Services;
using Jobs.Common.Constants;
using Jobs.Common.Extentions;
using Jobs.Common.Responses;
using Jobs.Common.SerializationSettings;
using Jobs.Core.Contracts;
using Jobs.Core.Helpers;
using Jobs.Dto.Request;
using Jobs.Entities.DataModel;
using Jobs.Entities.Responses;
using Keycloak.Client.Models;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Jobs.AccountApi.Features.Keycloak;

public static class Logout
{
    public record RequestLogoutCommand(LogoutUser User) : IRequest<bool>;
    
    public record Results(bool Result);
    
    public class LogoutProfile : Profile
    {
        public LogoutProfile()
        {
            CreateMap<LogoutUserDto , LogoutUser>().ReverseMap();
        }
    }
    
    public class LogoutCommandEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/logout", async Task<Results<Ok, BadRequest>> ([FromBody] LogoutUserDto user,
                HttpContext context,
                [FromServices] IHttpClientFactory httpClientFactory,
                [FromServices] IApiKeyService service, 
                [FromServices] IKeycloakAccountService accountService,
                [FromServices] ISender mediatr, 
                [FromServices] IPublisher publisher,
                [FromServices] IEncryptionService cryptService,
                [FromServices] ISignedNonceService signedNonceService,
                [FromServices] IHttpContextAccessor httpContextAccessor,
                [FromServices] IMapper mapper,
                [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey), Required, 
                 StringLength(HttpHeaderKeys.XApiHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiHeaderKeyMinLength)] string apiKey,
                [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey), Required, 
                 StringLength(HttpHeaderKeys.SNonceHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.SNonceHeaderKeyMinLength)] string signedNonce,
                [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey), Required, 
                 StringLength(HttpHeaderKeys.XApiSecretHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiSecretHeaderKeyMinLength)] string apiSecret) =>
            {
                Log.Information($"User Name: {user.Username}");
                Console.WriteLine($"UserAgent - {httpContextAccessor.HttpContext?.Request.Headers.UserAgent}");
                
                GuardsHelper.Guards(mediatr, service, cryptService, signedNonceService, httpContextAccessor);

                if (ApiSecurityHelper.IsBadRequest(httpContextAccessor, 
                        cryptService, signedNonceService, service, 
                        apiKey, signedNonce, apiSecret))
                {
                    return TypedResults.BadRequest();
                }
                
                var ipAddress = context.Request.GetIpAddress();
                Log.Information($"ClientIPAddress - {ipAddress}.");
                
                //POST /admin/realms/{realm}/users/{user-id}/logout
                //var result = await accountService.LogoutAsync(user).ConfigureAwait(false);
                var logoutEntityUser = mapper.Map<LogoutUser>(user);
                var result = await mediatr.Send(new RequestLogoutCommand(logoutEntityUser));
                return result ? TypedResults.Ok() : TypedResults.BadRequest();
            }).WithName("Logout")
            .WithOpenApi()
            .RequireRateLimiting("FixedWindow");
        }
    }
    
    public interface IKeycloakLogoutService
    {
        Task<bool> LogoutAsync(LogoutUser user);
    }
    
    public class KeycloakLogoutService(IHttpClientFactory httpClientFactory, 
        string baseUrl = "http://localhost:9001", string realm = "mjobs", 
        string adminUserName = "admin", string adminPassword = "newpwd") : 
        KeycloakAccountService(httpClientFactory, baseUrl, realm, adminUserName, adminPassword), IKeycloakLogoutService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly string _baseUrl = baseUrl;
        private readonly string _realm = realm;

        public async Task<bool> LogoutAsync(LogoutUser user)
        {
            var dataToken = await GetKeycloakAccessToken();
        
            Log.Information($"Admin AccessToken: {dataToken?.AccessToken}");

            var userInfo = await GetUserAsync(dataToken?.AccessToken!, user.Username);

            if (userInfo != null)
            {
                using var client = _httpClientFactory.CreateClient();
        
                client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", dataToken?.AccessToken);
            
                var url = $"{_baseUrl.TrimEnd('/')}/admin/realms/{_realm}/users/{userInfo.Id}/logout";
        
                Log.Information($"Logout URL - {url}");

                var logoutResponse= await client.PostAsync(url, null).ConfigureAwait(false);
                logoutResponse.EnsureSuccessStatusCode();

                if (logoutResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    Log.Information($"User - {userInfo.Username} logout ok.");
                    return true;
                }
            }

            return false;
        }
    }
    
    public class LogoutKeycloakUserCommandHandler(IKeycloakLogoutService service) : IRequestHandler<RequestLogoutCommand, bool>
    {
        public async Task<bool> Handle(RequestLogoutCommand command, CancellationToken cancellationToken) =>
            await service.LogoutAsync(command.User);
    }
}