using Jobs.Core.DataModel;
using Jobs.Core.Repositories;

namespace JobsWebApiNUnitTests;

public class DapperApiKeyRepositoryUnitTests
{
    private PostgresDapperApiKeyRepository _repository;
    
    [SetUp]
    public void Setup()
    {
        _repository = new PostgresDapperApiKeyRepository("Server=localhost;Port=5432;Database=jobs_db;User Id=admin;Password=newpwd;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=10");
    }
    
    [TearDown]
    public void Dispose()
    {
        _repository.Dispose();
    }

    [Test]
    public void AddApiKeyTest()
    {
        var current = new ApiKey
            { 
                Key = "safasdfasfas1241324321gbgfbf653636363542342342342asvavafafas", 
                Expiration = DateTime.UtcNow.AddHours(1) 
            };
        
        _repository.Add(current);
        _repository.Remove(current.Key);
        
        var result = _repository.Get(current.Key);
        Assert.IsNull(result);
    }
    
    [Test]
    public void GetApiKeyTest()
    {
        var current = new ApiKey
        { 
            Key = "safasdfasfas1241324321gbgfbf65363636354234234324234asdfsa2asvavafafas", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        _repository.Add(current);
        
        var result = _repository.Get(current.Key);
        Assert.IsNotNull(result);
        
        Assert.IsTrue(result.Key.Equals(current.Key));
        
        _repository.Remove(current.Key);
        
        var resultDeleted = _repository.Get(current.Key);
        Assert.IsNull(resultDeleted);
    }
    
    [Test]
    public void GetNotFoundApiKeyTest()
    {
        var result = _repository.Get("current.Key");
        Assert.IsNull(result);
    }
    
    [Test]
    public async Task AddApiKeyAsyncTest()
    {
        var current = new ApiKey
        { 
            Key = "safasdfasfas1241324321gbgfbf653636363542342342342asvavafafas97875957895", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        await _repository.AddAsync(current);
        await _repository.RemoveAsync(current.Key);
        
        var result = await _repository.GetAsync(current.Key);
        Assert.IsNull(result);
    }
    
    [Test]
    public async Task GetApiKeyAsyncTest()
    {
        var current = new ApiKey
        { 
            Key = "safasdfasfas1241324321gbgfbf65363636354234234324234asdfsa2asvavafafas", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        await _repository.AddAsync(current);
        
        var result = await _repository.GetAsync(current.Key);
        Assert.IsNotNull(result);
        
        Assert.IsTrue(result.Key.Equals(current.Key));
        
        await _repository.RemoveAsync(current.Key);
        
        var resultDeleted = await _repository.GetAsync(current.Key);
        Assert.IsNull(resultDeleted);
    }
    
    [Test]
    public void RemoveApiKeyTest()
    {
        var current = new ApiKey
        { 
            Key = "safasdfasfas1241324321gbgfbf65363636354234234324234asdfsa2asvavafafas", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        _repository.Add(current);
        
        var result = _repository.Get(current.Key);
        Assert.IsNotNull(result);
        
        Assert.IsTrue(result.Key.Equals(current.Key));
        
        _repository.Remove(current.Key);
        
        var resultDeleted = _repository.Get(current.Key);
        Assert.IsNull(resultDeleted);
        
        current = new ApiKey
        { 
            Key = "safasdfasfas12413253451251324`2143`24`2432`4`234124231", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        _repository.Add(current);
        
        result = _repository.Get(current.Key);
        Assert.IsNotNull(result);
        
        Assert.IsTrue(result.Key.Equals(current.Key));
        
        _repository.Remove(current.Key);
        
        resultDeleted = _repository.Get(current.Key);
        Assert.IsNull(resultDeleted);
        
        
        current = new ApiKey
        { 
            Key = "safasdfasfas12413253451251324243124322143241224412432321214234124231", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        _repository.Add(current);
        _repository.Add(current);
        _repository.Add(current);
        _repository.Add(current);
        
        result = _repository.Get(current.Key);
        Assert.IsNotNull(result);
        
        Assert.IsTrue(result.Key.Equals(current.Key));
        
        _repository.Remove(current.Key);
        
        resultDeleted = _repository.Get(current.Key);
        Assert.IsNull(resultDeleted);
        
    }
    
    [Test]
    public async Task RemoveApiKeyAsyncTest()
    {
        var current = new ApiKey
        { 
            Key = "safasdfasfas1241324321gbgfbf65363636354234234324234asdfsa2asvavafafas", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        await _repository.AddAsync(current);
        
        var result = await _repository.GetAsync(current.Key);
        Assert.IsNotNull(result);
        
        Assert.IsTrue(result.Key.Equals(current.Key));
        
        await _repository.RemoveAsync(current.Key);
        
        var resultDeleted = await _repository.GetAsync(current.Key);
        Assert.IsNull(resultDeleted);
        
        current = new ApiKey
        { 
            Key = "safasdfasfas12413253451251324`2143`24`2432`4`234124231", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        await _repository.AddAsync(current);
        
        result = await _repository.GetAsync(current.Key);
        Assert.IsNotNull(result);
        
        Assert.IsTrue(result.Key.Equals(current.Key));
        
        await _repository.RemoveAsync(current.Key);
        
        resultDeleted = await _repository.GetAsync(current.Key);
        Assert.IsNull(resultDeleted);
        
        
        current = new ApiKey
        { 
            Key = "safasdfasfas12413253451251324243124322143241224412432321214234124231", 
            Expiration = DateTime.UtcNow.AddHours(1) 
        };
        
        await _repository.AddAsync(current);
        await _repository.AddAsync(current);
        await _repository.AddAsync(current);
        await _repository.AddAsync(current);
        
        result = await _repository.GetAsync(current.Key);
        Assert.IsNotNull(result);
        
        Assert.IsTrue(result.Key.Equals(current.Key));
        
        await _repository.RemoveAsync(current.Key);
        
        resultDeleted = await _repository.GetAsync(current.Key);
        Assert.IsNull(resultDeleted);
        
    }
}