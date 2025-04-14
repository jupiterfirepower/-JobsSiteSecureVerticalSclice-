using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Jobs.AccountApi.Contracts;
using Jobs.Common.Constants;
using Jobs.Common.Extentions;
using Jobs.Core.Contracts;
using Jobs.Core.Extentions;
using Jobs.Core.Helpers;
using Jobs.Dto.Request;
using Jobs.Entities.DataModel;
using Jobs.Entities.Responses;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Jobs.AccountApi.Features.Keycloak;

public static class Login
{
    public record RequestLoginCommand(LoginUserDto Login) : IRequest<KeycloakTokenResponse>;
    
    public record Results(KeycloakTokenResponse Result);
    
    public class LoginCommandEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/login", async Task<Results<Ok<KeycloakTokenResponse>, BadRequest, NotFound>> ([FromBody] LoginUserDto user,
                HttpContext context,
                [FromServices] IKeycloakAccountService accountService,
                [FromServices] ISender mediatr,
                [FromServices] IPublisher publisher,
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
                Log.Information($"UserName: {user.Username} , Password: {user.Password}");
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

                try
                {
                    var result = await mediatr.Send(new RequestLoginCommand(user));
                    //var result = await accountService.LoginAsync(user.UserName, user.Password).ConfigureAwait(false);
                    return TypedResults.Ok(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return TypedResults.NotFound();
                }
            }).WithName("Login")
            .WithOpenApi()
            .AllowAnonymous()
            .RequireRateLimiting("FixedWindow");
        }
    }
    
    public interface IKeycloakLoginService
    {
        Task<KeycloakTokenResponse> LoginAsync(string username, string password);
    }
    
    public class KeycloakLoginService(IHttpClientFactory httpClientFactory, 
    string baseUrl = "http://localhost:9001", string realm = "mjobs", 
    string adminUserName = "admin", string adminPassword = "newpwd") : IKeycloakLoginService
    {
        public async Task<KeycloakTokenResponse> LoginAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
        
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            using var httpClient = httpClientFactory.CreateClient();
        
            // Form data is typically sent as key-value pairs
            var adminData = new List<KeyValuePair<string, string>>
            {
                new ("grant_type", "password"),
                new ("username", username),
                new ("password", password),
                new ("client_id", "admin-cli")
            };

            // Encodes the key-value pairs for the ContentType 'application/x-www-form-urlencoded'
            using var adminContent = new FormUrlEncodedContent(adminData);
        
            var adminResponse = await httpClient.PostAsync($"{baseUrl.TrimEnd('/')}/realms/{realm}/protocol/openid-connect/token", adminContent).ConfigureAwait(false);
            adminResponse.EnsureSuccessStatusCode();
            var result = await adminResponse.Content.ReadAsStringAsync();
        
            return JsonSerializer.Deserialize<KeycloakTokenResponse>(result)!;
        }
    }
    
    public class LoginKeycloakUserCommandHandler(IKeycloakLoginService service) : IRequestHandler<RequestLoginCommand, KeycloakTokenResponse>
    {
        public async Task<KeycloakTokenResponse> Handle(RequestLoginCommand command, CancellationToken cancellationToken) =>
            await service.LoginAsync(command.Login.Username, command.Login.Password);
    }
}