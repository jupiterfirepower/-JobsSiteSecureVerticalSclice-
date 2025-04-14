using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
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
using MediatR;
using Jobs.Dto.Request;
using Jobs.Entities.DataModel;
using Jobs.Entities.Responses;
using Keycloak.Client.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Jobs.AccountApi.Features.Keycloak;

public static class Register
{
    public record RequestRegisterCommand(UserDto User) : IRequest<RegisterUserResponse>;
    
    public record Results(RegisterUserResponse Result);
    
    public class RegisterKeycloakUserProfile : Profile
    {
        public RegisterKeycloakUserProfile()
        {
            CreateMap<UserDto , User>().ReverseMap();
        }
    }
    
    public class RegisterCommandEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/register", async Task<Results<Ok<RegisterUserResponse>, BadRequest>> ([FromBody] UserDto user,
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
                Log.Information($"User Email: {user.Email} , Password: {user.Password}");
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
                
                //var result = await accountService.RegisterUser(user).ConfigureAwait(false);
                var result = await mediatr.Send(new RequestRegisterCommand(user));

                return TypedResults.Ok(result);
            }).WithName("Register")
            .WithOpenApi()
            .AllowAnonymous()
            .RequireRateLimiting("FixedWindow");
        }
    }
    
    public interface IKeycloakRegisterService
    {
        Task<RegisterUserResponse> RegisterUser(UserDto user);
    }
    
    public class KeycloakRegisterService(IHttpClientFactory httpClientFactory, IMapper mapper,
        string baseUrl = "http://localhost:9001", string realm = "mjobs", 
        string adminUserName = "admin", string adminPassword = "newpwd") :
        KeycloakAccountService(httpClientFactory, baseUrl, realm, adminUserName, adminPassword), IKeycloakRegisterService
    {
        public async Task<RegisterUserResponse> RegisterUser(UserDto user)
        {
            var entityUser = mapper.Map<User>(user);
            return await RegisterEntityUser(entityUser);
        }

        private async Task<RegisterUserResponse> RegisterEntityUser(User user)
        {
            var dataToken = await GetKeycloakAccessToken();

            Log.Information($"Admin AccessToken: {dataToken?.AccessToken}");
        
            var userInfo = await GetUserAsync(dataToken?.AccessToken!, user.Email);

            if (userInfo == null) // user not found(not registered yet)
            {
                Console.WriteLine($"User not registered : {user.Email}, Password: {user.Password}");
                //var refreshedToken = await RefreshTokenAsync(dataToken.AccessToken);
            
                using var client = httpClientFactory.CreateClient();

                client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization =
                    //new AuthenticationHeaderValue("Bearer", refreshedToken?.AccessToken);
                    new AuthenticationHeaderValue("Bearer", dataToken?.AccessToken);

                var userData = new UserContext(user)
                {
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    UserName = user.Email,
                    Credentials = [new Credentials { Type = "password", Value = user.Password, Temporary = false }]
                };

                var requestJson = JsonSerializer.Serialize(userData);
                using var userContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                Log.Information($"Request Json: {requestJson}");

                var response = await client.PostAsync($"{baseUrl.TrimEnd('/')}/admin/realms/{realm}/users", userContent);
                response.EnsureSuccessStatusCode();

                Log.Information("User Created!");
            
                return new RegisterUserResponse { Result = true, IsAdded = true };
            }

            return new RegisterUserResponse { Result = false, IsAdded = false };
        }
    }
    
    public class RegisterKeycloakUserCommandHandler(IKeycloakRegisterService service) : IRequestHandler<RequestRegisterCommand, RegisterUserResponse>
    {
        public async Task<RegisterUserResponse> Handle(RequestRegisterCommand command, CancellationToken cancellationToken) =>
            await service.RegisterUser(command.User);
    }
}