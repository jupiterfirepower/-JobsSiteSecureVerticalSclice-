using Jobs.Core.DataModel;
using Jobs.Core.Services;

namespace JobsWebApiNUnitTests;

public class ConcurrentStorageServiceUnitTests
{
    private readonly ConcurrentStorageService<ApiKey> _storageService = ConcurrentStorageService<ApiKey>.GetInstance();
    
    [SetUp]
    public void Setup()
    {
    }
    
    [TearDown]
    public void Dispose()
    {
        _storageService.Clear();
    }
    
    [Test]
    public void AddApiKeyTest()
    {
        var current = new ApiKey
        { 
            Key = "safasdfasfas1241324321gbgfbf653636363542342342342asvavafafas", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        _storageService.Add(current.Key, current);
        var item = _storageService.Get(current.Key);
        Assert.IsNotNull(item);

        var result = _storageService.Remove(current.Key);
        Assert.IsTrue(result);
        
        item = _storageService.Get(current.Key);
        Assert.IsNull(item);
    }
    
    [Test]
    public void RemoveApiKeyTest()
    {
        var current = new ApiKey
        { 
            Key = "safasdfasfas1241324321gbgfbf653636363542342342342asvavafafas", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        _storageService.Add(current.Key, current);
        var item = _storageService.Get(current.Key);
        Assert.IsNotNull(item);

        var result = _storageService.Remove(current.Key);
        Assert.IsTrue(result);
        
        item = _storageService.Get(current.Key);
        Assert.IsNull(item);
    }
    
    [Test]
    public void RemoveApiKeyThatNotExistsTest()
    {
        var result = _storageService.Remove("123");
        Assert.IsFalse(result);
    }
    
    [Test]
    public void GetApiKeyTest()
    {
        var current = new ApiKey
        { 
            Key = "1234214324243245135134515135135134513513", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        _storageService.Add(current.Key, current);
        var item = _storageService.Get(current.Key);
        Assert.IsNotNull(item);

        var result = _storageService.Remove(current.Key);
        Assert.IsTrue(result);
        
        item = _storageService.Get(current.Key);
        Assert.IsNull(item);
    }
    
    [Test]
    public void ClearApiKeyTest()
    {
        var current = new ApiKey
        { 
            Key = "1234214324243245135134515135135134513513", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        _storageService.Add(current.Key, current);
        
        current = new ApiKey
        { 
            Key = "xbzbdgdagda1234214324243245135134515135135134513513", 
            Expiration = DateTime.UtcNow.AddMinutes(30)
        };
        
        _storageService.Add(current.Key, current);

        _storageService.Clear();
        
        var count = _storageService.GetAll().Count;
        Assert.IsTrue(count == 0);
    }
    
    [Test]
    public void AddOrUpdateApiKeyTest()
    {
        var first = new ApiKey
        { 
            Key = "1234214324243245135134515135135134513513", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        _storageService.Add(first.Key, first);

        var item = _storageService.Get(first.Key);
        Assert.IsNotNull(item);
        
        first.Expiration = DateTime.UtcNow.AddMinutes(30);
        
        _storageService.AddOrUpdate(first.Key, first);
        
        var item2 = _storageService.Get(first.Key);
        Assert.IsNotNull(item2);
        Assert.IsTrue(item2.Expiration == first.Expiration);
        
        _storageService.Clear();
        
        var count = _storageService.GetAll().Count;
        Assert.IsTrue(count == 0);
    }
}