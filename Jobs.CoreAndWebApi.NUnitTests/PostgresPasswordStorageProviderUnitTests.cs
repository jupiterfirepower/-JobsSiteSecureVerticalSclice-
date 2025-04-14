using Jobs.Core.DataModel;
using Jobs.Core.Providers;
using Jobs.Core.Repositories;

namespace JobsWebApiNUnitTests;

public class PostgresPasswordStorageProviderUnitTests
{
    private PostgresPasswordStorageProvider _storageProvider;
    
    [SetUp]
    public void Setup()
    {
        var repository = new PostgresDapperPasswordRepository("Server=localhost;Port=5432;Database=jobs_db;User Id=admin;Password=newpwd;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=10;");
        _storageProvider = new PostgresPasswordStorageProvider(repository);
    }
    
    [TearDown]
    public void Dispose()
    {
    }
    
    [Test]
    public void AddAndGetApiKeyTest()
    {
        var current = new ExternalUserCredential
        { 
            Email = "test@test.com", 
            Password = "12345678"
        };
        
        _storageProvider.AddUserCredential(current);
        var data = _storageProvider.GetUserCredential(current.Email);
        Assert.IsNotNull(data);
        Assert.IsTrue(data.Email==current.Email);
        Assert.IsTrue(data.Password==current.Password);
    }
}