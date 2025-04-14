using System.Net;
using Jobs.Core.Repositories;

namespace JobsWebApiNUnitTests;

public class ArangoDbAdditionalDbOperationsUnitTests
{
    private ArangoDbAdditionalDbOperations _operations;
    
    [SetUp]
    public void Setup()
    {
        _operations = new ArangoDbAdditionalDbOperations("http://localhost:8529", "root", "rootpassword", "jobs","pwdnew");
    }
    
    [TearDown]
    public void Dispose()
    {
        Task.Run(async () => await _operations.DeleteDbAsync());
    }
    
    [Test]
    public async Task CreateDbTest()
    {
        await _operations.CreateDbAsync();
        var result = await _operations.GetDatabasesAsync();

        Assert.False(result.Error);
        Assert.True(HttpStatusCode.OK == result.Code);
        Assert.True(result.Result.Any());
    }
    
    [Test]
    public async Task CreateAndDeleteDbTest()
    {
        await _operations.CreateDbAsync();
        var result = await _operations.GetDatabasesAsync();

        Assert.False(result.Error);
        Assert.True(HttpStatusCode.OK == result.Code);
        Assert.True(result.Result.Any());
        
        await _operations.DeleteDbAsync();
    }
    
    [Test]
    public async Task CreateCollectionTest()
    {
        await _operations.CreateDbAsync();
        var result = await _operations.GetDatabasesAsync();

        Assert.False(result.Error);
        Assert.True(HttpStatusCode.OK == result.Code);
        Assert.True(result.Result.Any());
        
        await _operations.CreateCollectionAsync();
        
        await _operations.DeleteDbAsync();
    }
}