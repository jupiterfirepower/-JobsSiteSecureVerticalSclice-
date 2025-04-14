using Jobs.Core.Contracts.Providers;
using Jobs.Core.Contracts.Repositories;
using Jobs.Core.DataModel;
using Jobs.Core.Providers;
using Jobs.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JobsWebApiNUnitTests;

public class PostgresApiKeyStorageServiceProviderUnitTests
{
    private ServiceProvider _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddScoped<IApiKeyStoreRepository, PostgresDapperApiKeyRepository>(x=>
            new PostgresDapperApiKeyRepository("Server=localhost;Port=5432;Database=jobs_db;User Id=admin;Password=newpwd"));
        services.AddScoped<IApiKeyStorageServiceProvider, PostgresApiKeyStorageServiceProvider>();
        

        _serviceProvider = services.BuildServiceProvider();
    }
    
    [TearDown]
    public void Dispose()
    {
        _serviceProvider.Dispose();
    }
    
    [Test]
    public void AddRemoveApiKeyTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var service = scopedServices.GetRequiredService<IApiKeyStorageServiceProvider>();
            var repository = scopedServices.GetRequiredService<IApiKeyStoreRepository>();
            
            var current = new ApiKey
                { Key = "safasdfasfas1241324321gbgfbf65363636354234234`234`2asvavafafas", Expiration = DateTime.UtcNow.AddHours(1) };
            service.AddApiKey(current);
            
            bool result = service.IsKeyValid(current.Key);
            Assert.IsTrue(result);
            
            repository.Remove(current.Key);
            bool resdel = service.IsKeyValid(current.Key);
            Assert.IsFalse(resdel);
        }
        Assert.Pass();
    }
}