using System.Reflection;
using Jobs.AccountApi.Features.Keycloak;
using Jobs.AccountApi.Services;
using Jobs.Common.Extentions;
using Jobs.Common.Options;
using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;
using Jobs.Core.Managers;
using Jobs.Core.Options;
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
        Console.WriteLine($"VAULT_ADDR: {vaultUri}");
        var vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN");
        
        var vaultSettings = configuration.GetSection("VaultSetting").Get<VaultOptions>();
        var vaultServerUri = vaultSettings?.VaultServerUrl + ":" + vaultSettings?.VaultServerPort;
        Console.WriteLine($"vaultSettings VaultServerUrl: {vaultServerUri}");
        
        CryptOptions cryptOptions = new();

        configuration
            .GetRequiredSection(nameof(CryptOptions))
            .Bind(cryptOptions);

        try
        {
            // Hashicorp Vault Secrets.
            var vaultSecretsProvider = new VaultSecretProvider(vaultServerUri ?? vaultUri, vaultToken);
            
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
            
            var taskThird = Task.Run(async () => await vaultSecretsProvider.GetSecretValueAsync("secrets/services/reference", "PKey", "secrets"));
            taskThird.Wait();
            
            var vaultPKey = taskThird.Result;
            cryptOptions.PKey = vaultPKey ?? cryptOptions.PKey;
            
            var taskFour = Task.Run(async () => await vaultSecretsProvider.GetSecretValueAsync("secrets/services/reference", "IV", "secrets"));
            taskFour.Wait();
            
            var vaultIv = taskFour.Result;
            cryptOptions.IV = vaultIv ?? cryptOptions.IV;
        }
        catch (VaultApiException e)
        {
            Console.WriteLine(e.Message);
        }

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