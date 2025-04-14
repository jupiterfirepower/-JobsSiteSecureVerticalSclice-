using System.Security.Cryptography;
using Jobs.Core.DataModel;
using Jobs.Core.Providers;
using Jobs.Core.Services;

namespace JobsWebApiNUnitTests;

public class MemoryPasswordStorageProviderUniTests
{
    private MemoryPasswordStorageProvider _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        using Aes aesAlgorithm = Aes.Create();
        aesAlgorithm.KeySize = 256;
        aesAlgorithm.GenerateKey();
            
        var crypto = new NaiveEncryptionService(aesAlgorithm.Key, aesAlgorithm.IV);
        _serviceProvider = new MemoryPasswordStorageProvider(crypto);
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
        
        _serviceProvider.AddUserCredential(current);
        var data = _serviceProvider.GetUserCredential(current.Email);
        Assert.IsNotNull(data);
        Assert.IsTrue(data.Email==current.Email);
        Assert.IsTrue(data.Password==current.Password);
    }
}