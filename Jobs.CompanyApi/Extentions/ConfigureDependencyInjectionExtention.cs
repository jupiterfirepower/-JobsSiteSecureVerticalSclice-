using System.Reflection;
using Jobs.Common.Contracts;
using Jobs.Common.Extentions;
using Jobs.Common.Options;
using Jobs.CompanyApi.Features.Companies;
using Jobs.CompanyApi.Repositories;
using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;
using Jobs.Core.Managers;
using Jobs.Core.Providers;
using Jobs.Core.Providers.Vault;
using Jobs.Core.Services;
using Jobs.Entities.Models;
using VaultSharp.Core;

namespace Jobs.CompanyApi.Extentions;

public static class ConfigureDependencyInjectionExtention
{
    public static void ConfigureDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        // user-secrets
        var companySecretKey = configuration["CompanyApiService:SecretKey"];
        Console.WriteLine($"companySecretKey: {companySecretKey}");
        var companyServiceDefApiKey = configuration["CompanyApiService:DefaultApiKey"];
        Console.WriteLine($"companyServiceDefApiKey: {companyServiceDefApiKey}");
        
        var vaultUri = Environment.GetEnvironmentVariable("VAULT_ADDR");
        var vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN");

        try
        {
            // Hashicorp Vault Secrets.
            var vaultSecretsProvider = new VaultSecretProvider(vaultUri, vaultToken);
            
            var task = Task.Run(async () => await vaultSecretsProvider.GetSecretValueAsync("secrets/services/company", "SecretKey", "secrets"));
            task.Wait();
            var vaultSecretKey = task.Result;
            
            companySecretKey ??= vaultSecretKey;
            
            var taskSecond = Task.Run(async () => await vaultSecretsProvider.GetSecretValueAsync("secrets/services/company", "DefaultApiKey", "secrets"));
            taskSecond.Wait();
            var vaultDefaultApiKey = taskSecond.Result;

            companyServiceDefApiKey ??= vaultDefaultApiKey;
            
            Console.WriteLine($"vacancySecretKey: {companySecretKey}");
            Console.WriteLine($"vacancyServiceDefApiKey: {companyServiceDefApiKey}");
        }
        catch (VaultApiException e)
        {
            Console.WriteLine(e.Message);
        }
        
        CryptOptions cryptOptions = new();

        configuration
            .GetRequiredSection(nameof(CryptOptions))
            .Bind(cryptOptions);

        services.AddScoped<IGenericRepository<Company>, CompanyRepository>();
        services.AddScoped<IApiKeyStorageServiceProvider, MemoryApiKeyStorageServiceProvider>();
        //builder.Services.AddScoped<IApiKeyManagerServiceProvider, ApiKeyManagerServiceProvider>();
        services.AddScoped<IApiKeyManagerServiceProvider, ApiKeyManagerServiceProvider>(p =>
        {
            var currentService = p.ResolveWith<ApiKeyManagerServiceProvider>();
            currentService.AddApiKey(new ApiKey { Key = companyServiceDefApiKey, Expiration = null });
            return currentService;
        });
        services.AddScoped<ISecretApiKeyRepository, SecretApiKeyRepository>();

        services.AddScoped<GetCompanies.ICompaniesService, GetCompanies.CompaniesService>();
        services.AddScoped<GetCompany.ICompanyService, GetCompany.CompanyService>();
        services.AddScoped<CreateCompany.ICreateCompanyService, CreateCompany.CreateCompanyService>();
        services.AddScoped<UpdateCompany.IUpdateCompanyService, UpdateCompany.UpdateCompanyService>();
        services.AddScoped<DeleteCompany.IDeleteCompanyService, DeleteCompany.DeleteCompanyService>();

        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<ISignedNonceService, SignedNonceService>();
        services.AddScoped<IEncryptionService, NaiveEncryptionService>(p =>
            p.ResolveWith<NaiveEncryptionService>(Convert.FromBase64String(cryptOptions.PKey),
                Convert.FromBase64String(cryptOptions.IV)));
        services.AddScoped<ISecretApiService, SecretApiService>(p =>
            p.ResolveWith<SecretApiService>(companySecretKey));
        
        services.AddSingleton<PeriodicHostedService>();
        services.AddHostedService(provider => provider.GetRequiredService<PeriodicHostedService>());

        services.AddAutoMapper(Assembly.GetExecutingAssembly()); // AutoMapper registration
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    }
}