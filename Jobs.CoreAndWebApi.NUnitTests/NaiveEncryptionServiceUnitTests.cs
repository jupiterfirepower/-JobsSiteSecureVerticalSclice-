using System.Security.Cryptography;
using Jobs.Core.Services;

namespace JobsWebApiNUnitTests;

public class NaiveEncryptionServiceUnitTests
{
    [Test]
    public void AddRemoveApiKeyTest()
    {
        using var aesAlgorithm = Aes.Create();
        
        aesAlgorithm.KeySize = 256;
        aesAlgorithm.GenerateKey();
        string keyBase64 = Convert.ToBase64String(aesAlgorithm.Key);
        string ivBase64 = Convert.ToBase64String(aesAlgorithm.IV.ToArray());

        var key = Convert.FromBase64String(keyBase64);
        var iv = Convert.FromBase64String(ivBase64);
        var cryptoProvider = new NaiveEncryptionService(key, iv);
            
        var testData = "TestData";
        var cryptData = cryptoProvider.Encrypt(testData);

        var decryptedResult = cryptoProvider.Decrypt(cryptData);
        Assert.IsNotNull(decryptedResult);
        Assert.IsTrue(decryptedResult.Equals(testData));
        
        Assert.Pass();
    }
}