using System.Reflection;
using Jobs.Common.Contracts;
using Jobs.Common.Extentions;
using Jobs.Common.Options;
using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;
using Jobs.Core.Managers;
using Jobs.Core.Providers;
using Jobs.Core.Providers.Vault;
using Jobs.Core.Services;
using Jobs.Entities.Models;
using Jobs.ReferenceApi.Contracts;
using Jobs.ReferenceApi.Features.Categories;
using Jobs.ReferenceApi.Features.EmploymentTypes;
using Jobs.ReferenceApi.Features.WorkTypes;
using Jobs.ReferenceApi.Repositories;
using Jobs.ReferenceApi.Services;
using VaultSharp.Core;

namespace Jobs.ReferenceApi.Extentions;

public static class ConfigureDependencyInjectionExtention
{
    public static void ConfigureDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        // user-secrets
        var referenceSecretKey = configuration["ReferenceApiService:SecretKey"];
        Console.WriteLine($"referenceSecretKey: {referenceSecretKey}");
        var referenceServiceDefApiKey = configuration["ReferenceApiService:DefaultApiKey"];
        Console.WriteLine($"referenceServiceDefApiKey: {referenceServiceDefApiKey}");
        
        var vaultUri = Environment.GetEnvironmentVariable("VAULT_ADDR");
        var vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN");

        try
        {
            // Hashicorp Vault Secrets.
            var vaultSecretsProvider = new VaultSecretProvider(vaultUri, vaultToken);
            
            var task = Task.Run(async () => await vaultSecretsProvider.GetSecretValueAsync("secrets/services/reference", "SecretKey", "secrets"));
            task.Wait();
            var vaultSecretKey = task.Result;
            
            referenceSecretKey ??= vaultSecretKey;
            
            var taskSecond = Task.Run(async () => await vaultSecretsProvider.GetSecretValueAsync("secrets/services/reference", "DefaultApiKey", "secrets"));
            taskSecond.Wait();
            var vaultDefaultApiKey = taskSecond.Result;

            referenceServiceDefApiKey ??= vaultDefaultApiKey;
            
            Console.WriteLine($"vacancySecretKey: {referenceSecretKey}");
            Console.WriteLine($"vacancyServiceDefApiKey: {referenceServiceDefApiKey}");
        }
        catch (VaultApiException e)
        {
            Console.WriteLine(e.Message);
        }
        
        CryptOptions cryptOptions = new();

        configuration
            .GetRequiredSection(nameof(CryptOptions))
            .Bind(cryptOptions);
        
        services.AddSingleton<ICacheService, LocalCacheService>();
        
        services.AddScoped<IGenericRepository<WorkType>, WorkTypeRepository>();
        services.AddScoped<IGenericRepository<EmploymentType>, EmploymentTypeRepository>();
        services.AddScoped<IGenericRepository<Category>, CategoryRepository>();
        services.AddScoped<IApiKeyStorageServiceProvider, MemoryApiKeyStorageServiceProvider>();
        //builder.Services.AddScoped<IApiKeyManagerServiceProvider, ApiKeyManagerServiceProvider>();
        services.AddScoped<IApiKeyManagerServiceProvider, ApiKeyManagerServiceProvider>(p =>
        {
            var currentService = p.ResolveWith<ApiKeyManagerServiceProvider>();
            currentService.AddApiKey(new ApiKey { Key = referenceServiceDefApiKey, Expiration = null });
            return currentService;
        });
        services.AddScoped<ISecretApiKeyRepository, SecretApiKeyRepository>();
        
        services.AddScoped<Categories.ICategoryService, Categories.CategoryService>();
        services.AddScoped<WorkTypes.IWorkTypeService, WorkTypes.WorkTypeService>();
        services.AddScoped<GetWorkTypeById.IWorkTypeServiceExtended, GetWorkTypeById.WorkTypeServiceExtended>();
        services.AddScoped<EmploymentTypes.IEmploymentTypeService, EmploymentTypes.EmploymentTypeService>();
        services.AddScoped<GetEmploymentTypeById.IEmploymentTypeServiceExtended, GetEmploymentTypeById.EmploymentTypeServiceExtended>();
        
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IEncryptionService, NaiveEncryptionService>(p => 
            p.ResolveWith<NaiveEncryptionService>(Convert.FromBase64String(cryptOptions.PKey), Convert.FromBase64String(cryptOptions.IV)));
        services.AddScoped<ISignedNonceService, SignedNonceService>();
        services.AddScoped<ISecretApiService, SecretApiService>(p => p.ResolveWith<SecretApiService>(referenceSecretKey));
        
        services.AddSingleton<PeriodicHostedService>();
        services.AddHostedService(provider => provider.GetRequiredService<PeriodicHostedService>());
        
        services.AddAutoMapper(Assembly.GetExecutingAssembly()); // AutoMapper registration
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    }
}