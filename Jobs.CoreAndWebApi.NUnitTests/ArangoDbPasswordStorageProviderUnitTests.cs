using System.Net;
using Jobs.Core.DataModel;
using Jobs.Core.Providers;
using Jobs.Core.Repositories;

namespace JobsWebApiNUnitTests;

public class ArangoDbPasswordStorageProviderUnitTests
{
    private ArangoDbAdditionalDbOperations _operations;
    private const string Username = "jobs";
    private const string Password = "pwdnew";
    private const string DbName = "apikeydb";
    private const string ColName = "pwd";
    
    [SetUp]
    public void Setup()
    {
        _operations = new ArangoDbAdditionalDbOperations("http://localhost:8529", 
            "root", "rootpassword", Username,Password, DbName, ColName);
        
        var repository = new ArangoDbPasswordRepository("http://localhost:8529", Username, Password , DbName, ColName);
        
        var task = Task.Run(async () => await _operations.RemoveAllDatabasesAsync());
        task.GetAwaiter().GetResult();
    }
    
    [TearDown]
    public void Dispose()
    {
        Task.Run(async () => await _operations.DeleteDbAsync());
    }
    
    [Test]
    public async Task AddCredentialPasswordTest()
    {
        await _operations.CreateDbAsync();
        
        var result = await _operations.GetDatabasesAsync();

        Assert.False(result.Error);
        Assert.True(HttpStatusCode.OK == result.Code);
        Assert.True(result.Result.Any());
        Assert.True(result.Result.FirstOrDefault(x=>x.Equals(DbName)) != null);

        await _operations.CreateCollectionAsync();
        
        var repository = new ArangoDbPasswordRepository("http://localhost:8529", Username, Password , DbName, ColName);
        //var storage = new ArangoDbPasswordStorageProvider(repository);
        
        var item = new ExternalUserCredential { Email = "proc@gmail.com", Password = "pwdnew"};
        await repository.AddAsync(item);
        
        repository.Dispose();
        await _operations.DeleteDbAsync();
    }
    
    [Test]
    public async Task AddAndGetCredentialPasswordTest()
    {
        await _operations.CreateDbAsync();
        
        var result = await _operations.GetDatabasesAsync();

        Assert.False(result.Error);
        Assert.True(HttpStatusCode.OK == result.Code);
        Assert.True(result.Result.Any());
        Assert.True(result.Result.FirstOrDefault(x=>x.Equals(DbName)) != null);

        await _operations.CreateCollectionAsync();
        
        var repository = new ArangoDbPasswordRepository("http://localhost:8529", Username, Password , DbName, ColName);
        //var storage = new ArangoDbPasswordStorageProvider(repository);
        
        var item = new ExternalUserCredential { Email = "proc@gmail.com", Password = "pwdnew"};
        await repository.AddAsync(item);
        
        var tmp = await repository.GetAsync(item.Email);
        Assert.NotNull(tmp);
        
        repository.Dispose();
        await _operations.DeleteDbAsync();
    }
}