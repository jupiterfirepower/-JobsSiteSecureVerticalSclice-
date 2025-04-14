using System.Net;
using Jobs.Core.DataModel;
using Jobs.Core.Repositories;

namespace JobsWebApiNUnitTests;

public class ArangoDbApiKeyRepositoryUnitTests
{
    private ArangoDbAdditionalDbOperations _operations;
    private const string Username = "jobs";
    private const string Password = "pwdnew";
    private const string DbName = "apikey-db";
    private const string ColName = "dkeys";
    

    public ArangoDbApiKeyRepositoryUnitTests()
    {
        //_repository = null;
        _operations = new ArangoDbAdditionalDbOperations("http://localhost:8529", 
            "root", "rootpassword", 
            Username,Password, DbName, ColName);
    }
    
    [SetUp]
    public void Setup()
    {
        var task = Task.Run(async () => await _operations.RemoveAllDatabasesAsync());
        task.GetAwaiter().GetResult();
    }
    
    [TearDown]
    public void Dispose()
    {
        Task.Run(async () => await _operations.DeleteDbAsync());
    }
    
    [Test]
    public async Task AddApiKeyTest()
    {
        await _operations.CreateDbAsync();
        
        var result = await _operations.GetDatabasesAsync();

        Assert.False(result.Error);
        Assert.True(HttpStatusCode.OK == result.Code);
        Assert.True(result.Result.Any());
        Assert.True(result.Result.FirstOrDefault(x=>x.Equals(DbName)) != null);

        await _operations.CreateCollectionAsync();
        
        var repository = new ArangoDbApiKeyRepository("http://localhost:8529", Username, Password , DbName, ColName);
        
        var item = new ApiKey { Key = "98742525451345134513513514351513541513", Expiration = null };
        await repository.AddAsync(item);
        
        repository.Dispose();
        await _operations.DeleteDbAsync();
    }
    
    [Test]
    public async Task AddRemoveApiKeyTest()
    {
        await _operations.CreateDbAsync();
        
        var result = await _operations.GetDatabasesAsync();

        Assert.False(result.Error);
        Assert.True(HttpStatusCode.OK == result.Code);
        Assert.True(result.Result.Any());

        await _operations.CreateCollectionAsync();
        
        var repository = new ArangoDbApiKeyRepository("http://localhost:8529", Username, Password , DbName, ColName);

        var item = new ApiKey { Key = "3454315132525451345134513513514351513541513", Expiration = DateTime.UtcNow.AddHours(1) };

        await repository.AddAsync(item);
        
        var r = await repository.GetAsync(item.Key);
        Assert.NotNull(r);
        Assert.True(r.Key == item.Key);
        
        await repository.RemoveAsync(item.Key);
        
        var resultApiKey = await repository.GetAsync(item.Key);
        Assert.True(resultApiKey == null);
        
        repository.Dispose();
        
        await _operations.DeleteDbAsync();
    }
}