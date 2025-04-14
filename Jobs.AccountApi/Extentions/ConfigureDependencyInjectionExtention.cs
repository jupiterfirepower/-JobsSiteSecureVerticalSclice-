using System.Reflection;
using Jobs.AccountApi.Features.Keycloak;
using Jobs.AccountApi.Services;
using Jobs.Common.Extentions;
using Jobs.Common.Options;
using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;
using Jobs.Core.Managers;
using Jobs.Core.Providers;
using Jobs.Core.Providers.Vault;
using Jobs.Core.Services;
using VaultSharp.Core;

namespace Jobs.AccountApi.Extentions;

public static class ConfigureDependencyInjectionExtention
{
    public static void ConfigureDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        // user-secrets
        var accountServiceSecretKey = configuration["AccountApiService:SecretKey"];
        Console.WriteLine($"accountServiceSecretKey: {accountServiceSecretKey}");
        var accountServiceDefApiKey = configuration["AccountApiService:DefaultApiKey"];
        Console.WriteLine($"accountServiceDefApiKey: {accountServiceDefApiKey}");
        
        var vaultUri = Environment.GetEnvironmentVariable("VAULT_ADDR");
        var vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN");

        try
        {
            // Hashicorp Vault Secrets.
            var vaultSecretsProvider = new VaultSecretProvider(vaultUri, vaultToken);
            
            var task = Task.Run(async () => await vaultSecretsProvider.GetSecretValueAsync("secrets/services/account", "SecretKey", "secrets"));
            task.Wait();
            var vaultSecretKey = task.Result;
            
            accountServiceSecretKey ??= vaultSecretKey;
            
            var taskSecond = Task.Run(async () => await vaultSecretsProvider.GetSecretValueAsync("secrets/services/account", "DefaultApiKey", "secrets"));
            taskSecond.Wait();
            var vaultDefaultApiKey = taskSecond.Result;

            accountServiceDefApiKey ??= vaultDefaultApiKey;
            
            Console.WriteLine($"accountServiceSecretKey: {accountServiceSecretKey}");
            Console.WriteLine($"accountServiceDefApiKey: {accountServiceDefApiKey}");
        }
        catch (VaultApiException e)
        {
            Console.WriteLine(e.Message);
        }
        
        CryptOptions cryptOptions = new();

        configuration
            .GetRequiredSection(nameof(CryptOptions))
            .Bind(cryptOptions);
        
        services.AddScoped<IApiKeyStorageServiceProvider, MemoryApiKeyStorageServiceProvider>();
        //builder.Services.AddScoped<IApiKeyManagerServiceProvider, ApiKeyManagerServiceProvider>();
        services.AddScoped<IApiKeyManagerServiceProvider, ApiKeyManagerServiceProvider>(p =>
        {
            var currentService = p.ResolveWith<ApiKeyManagerServiceProvider>();
            currentService.AddApiKey(new ApiKey { Key = accountServiceDefApiKey, Expiration = null });
            return currentService;
        });

        //builder.Services.AddScoped<IKeycloakAccountService, KeycloakAccountService>();
        services.AddScoped<Login.IKeycloakLoginService, Login.KeycloakLoginService>();
        services.AddScoped<Logout.IKeycloakLogoutService, Logout.KeycloakLogoutService>();
        services.AddScoped<RefreshToken.IKeycloakRefreshTokenService, RefreshToken.KeycloakRefreshTokenService>();
        services.AddScoped<Register.IKeycloakRegisterService, Register.KeycloakRegisterService>();

        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IEncryptionService, NaiveEncryptionService>(p => 
            p.ResolveWith<NaiveEncryptionService>(Convert.FromBase64String(cryptOptions.PKey), Convert.FromBase64String(cryptOptions.IV)));
        services.AddScoped<ISignedNonceService, SignedNonceService>();
        services.AddScoped<ISecretApiService, SecretApiService>(p => 
            p.ResolveWith<SecretApiService>(accountServiceSecretKey));

        services.AddSingleton<PeriodicHostedService>();
        services.AddHostedService(provider => provider.GetRequiredService<PeriodicHostedService>());

        services.AddAutoMapper(Assembly.GetExecutingAssembly()); // AutoMapper registration
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    }
}