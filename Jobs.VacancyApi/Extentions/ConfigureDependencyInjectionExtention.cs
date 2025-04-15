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
using Jobs.VacancyApi.Contracts;
using Jobs.VacancyApi.Features.Vacancies;
using Jobs.VacancyApi.Repository;
using VaultSharp.Core;

namespace Jobs.VacancyApi.Extentions;

public static class ConfigureDependencyInjectionExtention
{
    public static void ConfigureDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        // user-secrets
        var vacancySecretKey = configuration["VacancyApiService:SecretKey"];
        Console.WriteLine($"vacancySecretKey: {vacancySecretKey}");
        var vacancyServiceDefApiKey = configuration["VacancyApiService:DefaultApiKey"];
        Console.WriteLine($"vacancyServiceDefApiKey: {vacancyServiceDefApiKey}");
        
        var vaultUri = Environment.GetEnvironmentVariable("VAULT_ADDR");
        var vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN");

        try
        {
            // Hashicorp Vault Secrets.
            var vaultSecretsProvider = new VaultSecretProvider(vaultUri, vaultToken);
            
            var task = Task.Run(async () => await vaultSecretsProvider.GetSecretValueAsync("secrets/services/vacancy", "SecretKey", "secrets"));
            task.Wait();
            var vaultSecretKey = task.Result;
            
            vacancySecretKey ??= vaultSecretKey;
            
            var taskSecond = Task.Run(async () => await vaultSecretsProvider.GetSecretValueAsync("secrets/services/vacancy", "DefaultApiKey", "secrets"));
            taskSecond.Wait();
            var vaultDefaultApiKey = taskSecond.Result;

            vacancyServiceDefApiKey ??= vaultDefaultApiKey;
            
            Console.WriteLine($"vacancySecretKey: {vacancySecretKey}");
            Console.WriteLine($"vacancyServiceDefApiKey: {vacancyServiceDefApiKey}");
        }
        catch (VaultApiException e)
        {
            Console.WriteLine(e.Message);
        }
        
        CryptOptions cryptOptions = new();

        configuration
            .GetRequiredSection(nameof(CryptOptions))
            .Bind(cryptOptions);
        
        

        services.AddScoped<IGenericRepository<Vacancy>, VacancyRepository>();
    
        services.AddScoped<GetVacancies.IVacanciesService, GetVacancies.VacanciesService>();
        services.AddScoped<GetVacancy.IVacancyService, GetVacancy.GetVacancyService>();
        services.AddScoped<CreateVacancy.ICreateVacancyService, CreateVacancy.CreateVacancyService>();
        services.AddScoped<UpdateVacancy.IUpdateVacancyService, UpdateVacancy.UpdateVacancyService>();
        services.AddScoped<DeleteVacancy.IDeleteVacancyService, DeleteVacancy.DeleteVacancyService>();
    
        services.AddScoped<IApiKeyStorageServiceProvider, MemoryApiKeyStorageServiceProvider>();
        services.AddScoped<IApiKeyManagerServiceProvider, ApiKeyManagerServiceProvider>(p =>
        {
            var currentService = p.ResolveWith<ApiKeyManagerServiceProvider>();
            currentService.AddApiKey(new ApiKey { Key = vacancyServiceDefApiKey, Expiration = null });
            return currentService;
        });
        services.AddScoped<ISecretApiKeyRepository, SecretApiKeyRepository>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IEncryptionService, NaiveEncryptionService>(p => 
            p.ResolveWith<NaiveEncryptionService>(Convert.FromBase64String(cryptOptions.PKey), Convert.FromBase64String(cryptOptions.IV)));
        services.AddScoped<ISignedNonceService, SignedNonceService>();
        services.AddScoped<ISecretApiService, SecretApiService>(p => 
            p.ResolveWith<SecretApiService>(vacancySecretKey));
        
        services.AddSingleton<PeriodicHostedService>();
        services.AddHostedService(provider => provider.GetRequiredService<PeriodicHostedService>());
    
        services.AddAutoMapper(Assembly.GetExecutingAssembly()); // AutoMapper registration
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    }
}