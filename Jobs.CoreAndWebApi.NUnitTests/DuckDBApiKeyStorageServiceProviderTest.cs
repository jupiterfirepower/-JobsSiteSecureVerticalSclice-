using Jobs.Core.DataModel;
using Jobs.Core.Providers;

namespace JobsWebApiNUnitTests;

public class DuckDbApiKeyStorageServiceProviderTest
{
    [Test]
    public void AddApiKeyTest()
    {
        var dbProvider = new DuckDbApiKeyStorageServiceProvider();
        var key = new ApiKey { Key = Guid.NewGuid().ToString(), Expiration = DateTime.Now.AddMinutes(15) };
        dbProvider.AddApiKey(key);
        Assert.IsNotNull(key);
    }
    
    [Test]
    public async Task AddApiKeyAsyncTest()
    {
        var dbProvider = new DuckDbApiKeyStorageServiceProvider();
        var key = new ApiKey { Key = Guid.NewGuid().ToString(), Expiration = DateTime.Now.AddMinutes(15) };
        await dbProvider.AddApiKeyAsync(key);
        Assert.IsNotNull(key);
    }
    
    [Test]
    public void AddAndGetApiKeyTest()
    {
        var dbProvider = new DuckDbApiKeyStorageServiceProvider();
        var key = new ApiKey { Key = Guid.NewGuid().ToString(), Expiration = DateTime.Now.AddMinutes(15) };
        dbProvider.AddApiKey(key);
        Assert.IsNotNull(key);
        var result = dbProvider.IsKeyValid(key.Key);
        Assert.IsTrue(result);
    }
    
    [Test]
    public async Task AddAndGetAsyncApiKeyTest()
    {
        var dbProvider = new DuckDbApiKeyStorageServiceProvider();
        var key = new ApiKey { Key = Guid.NewGuid().ToString(), Expiration = DateTime.Now.AddMinutes(15) };
        await dbProvider.AddApiKeyAsync(key);
        Assert.IsNotNull(key);
        var result = await dbProvider.IsKeyValidAsync(key.Key);
        Assert.IsTrue(result);
    }
}