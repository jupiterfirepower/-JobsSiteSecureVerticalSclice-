using Jobs.Core.DataModel;
using Jobs.Core.Repositories;

namespace JobsWebApiNUnitTests;

public class DapperPasswordRepositoryUnitTests
{
    private PostgresDapperPasswordRepository _repository;
    
    [SetUp]
    public void Setup()
    {
        // Password=newpwd;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=100
        _repository = new PostgresDapperPasswordRepository("Server=localhost;Port=5432;Database=jobs_db;User Id=admin;Password=newpwd;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=10;");
    }
    
    [TearDown]
    public void Dispose()
    {
        _repository.Dispose();
    }
    
    [Test]
    public void AddAndGetApiKeyTest()
    {
        var current = new ExternalUserCredential
        { 
            Email = "test@test.com", 
            Password = "12345678"
        };
        
        _repository.Add(current);
        var data = _repository.Get(current.Email);
        Assert.IsNotNull(data);
    }
    
    [Test]
    public async Task AddAndGetApiKeyAsyncTest()
    {
        var current = new ExternalUserCredential
        { 
            Email = "test1@test.com", 
            Password = "1234567890"
        };
        
        await _repository.AddAsync(current);
        var data = await _repository.GetAsync(current.Email);
        Assert.IsNotNull(data);
    }
}