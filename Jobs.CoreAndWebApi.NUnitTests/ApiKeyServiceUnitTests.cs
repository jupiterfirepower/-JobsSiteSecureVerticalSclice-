using System.Reflection;
using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;
using Jobs.Core.Providers;
using Jobs.Core.Services;
using Jobs.VacancyApi.Contracts;
using Jobs.VacancyApi.Data;
using Jobs.VacancyApi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Jobs.Core.Managers;

namespace JobsWebApiNUnitTests;

public class ApiKeyServiceUnitTests
{
    private ServiceProvider _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        // Using In-Memory database for testing
        services.AddDbContext<JobsDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
        
        services.AddAutoMapper(Assembly.GetExecutingAssembly()); 
            
        services.AddScoped<IApiKeyStorageServiceProvider, MemoryApiKeyStorageServiceProvider>(x => new MemoryApiKeyStorageServiceProvider());
        services.AddScoped<ISecretApiKeyRepository, SecretApiKeyRepository>();
        services.AddScoped<IApiKeyManagerServiceProvider, ApiKeyManagerServiceProvider>();
        services.AddScoped<ISecretApiService, SecretApiService>(x=> new SecretApiService("12345678"));
        services.AddScoped<IApiKeyService, ApiKeyService>();

        _serviceProvider = services.BuildServiceProvider();
    }
    
    [TearDown]
    public void Dispose()
    {
        _serviceProvider.Dispose();
    }
    
    [Test]
    public void ApiKeyServiceIsApiKeyValidTest()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<IApiKeyService>();
        var context = scopedServices.GetRequiredService<JobsDbContext>();
        var serviceProvider = scopedServices.GetRequiredService<IApiKeyStorageServiceProvider>();

        /*
        var testApiKey = new SecretApiKey
        {
            KeyId = 1,
            Key = "12345678",
            IsActive = true,
            Created = DateTime.UtcNow
        };
        
        context.ApiKeys.Add(testApiKey);
        context.SaveChanges();
        */
        var tapiKey = new ApiKey
        {
            Key = "12345678",
            Expiration = DateTime.UtcNow.AddDays(3)
        };
        serviceProvider.AddApiKey(tapiKey);

        var valid = service.IsValidApiKey("12345678");
        Assert.IsTrue(valid);
        
        var apiKey = service.GenerateApiKeyAsync().Result;
        Assert.IsNotNull(apiKey);
        Assert.IsTrue(apiKey.Key.Length >= 25);
        Assert.IsTrue(apiKey.Expiration > DateTime.UtcNow);
        
        var validApiKey = service.IsValidApiKey(apiKey.Key);
        Assert.IsTrue(validApiKey);
    }
    
    [Test]
    public void ProcessingServiceCreateVacancyTest()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<IApiKeyService>();

        var apiKey = service.GenerateApiKeyAsync().Result;
        Assert.IsNotNull(apiKey);
        Assert.IsTrue(apiKey.Key.Length >= 25);
        Assert.IsTrue(apiKey.Expiration > DateTime.UtcNow);
        
        var apiKey1 = service.GenerateApiKeyAsync().Result;
        Assert.IsNotNull(apiKey1);
        Assert.IsTrue(apiKey1.Key.Length >= 25);
        Assert.IsTrue(apiKey1.Expiration > DateTime.UtcNow);
        
        var apiKey2 = service.GenerateApiKeyAsync().Result;
        Assert.IsNotNull(apiKey2);
        Assert.IsTrue(apiKey2.Key.Length >= 25);
        Assert.IsTrue(apiKey2.Expiration > DateTime.UtcNow);
        
        var apiKey3 = service.GenerateApiKeyAsync().Result;
        Assert.IsNotNull(apiKey3);
        Assert.IsTrue(apiKey3.Key.Length >= 25);
        Assert.IsTrue(apiKey3.Expiration > DateTime.UtcNow);
        
        var apiKey4 = service.GenerateApiKeyAsync().Result;
        Assert.IsNotNull(apiKey4);
        Assert.IsTrue(apiKey4.Key.Length >= 25);
        Assert.IsTrue(apiKey4.Expiration > DateTime.UtcNow);
        
        var apiKey5 = service.GenerateApiKeyAsync().Result;
        Assert.IsNotNull(apiKey5);
        Assert.IsTrue(apiKey5.Key.Length >= 25);
        Assert.IsTrue(apiKey5.Expiration > DateTime.UtcNow);
    }
}