using Jobs.Core.DataModel;
using Jobs.Core.Providers;

namespace JobsWebApiNUnitTests;

public class MemoryApiKeyStorageServiceProviderUnitTests
{
    private MemoryApiKeyStorageServiceProvider _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        _serviceProvider = new MemoryApiKeyStorageServiceProvider();
    }
    
    [TearDown]
    public void Dispose()
    {
    }
    
    [Test]
    public void AddAndGetApiKeyTest()
    {
        var current = new ApiKey
        { 
            Key = "safasdfasfas1241324321gbgfbf653636363542342342342asvavafafas", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        _serviceProvider.AddApiKey(current);
        var valid = _serviceProvider.IsKeyValid(current.Key);

        Assert.IsTrue(valid);
    }
    
    [Test]
    public async Task AddAndGetApiKeyAsyncTest()
    {
        var current = new ApiKey
        { 
            Key = "safasdfasfas1241324321gbgfbf653636363542342342342asvavafafas", 
            Expiration = DateTime.UtcNow.AddMinutes(30)
        };
        
        await _serviceProvider.AddApiKeyAsync(current);
        var valid = await _serviceProvider.IsKeyValidAsync(current.Key);

        Assert.IsTrue(valid);
    }
}