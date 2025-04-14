using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;
using Jobs.Core.Helpers;

namespace Jobs.Core.Services;

public class ApiKeyService(IApiKeyManagerServiceProvider managerServiceProvider, ISecretApiService secretService) : IApiKeyService
{
    public async Task<ApiKey> GenerateApiKeyAsync()
    {
        var apiKey = new ApiKey
        {
            Key = ApiKeyGeneratorHelper.GenerateApiKey(),
            Expiration = DateTime.UtcNow.AddMinutes(30)
        };
        
        var result = await managerServiceProvider.AddApiKeyAsync(apiKey);
        Console.WriteLine($"GenerateApiKeyAsync {result}");
        return apiKey;
    }

    public bool IsValidApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) ) //|| 
            //(!string.IsNullOrEmpty(apiKey) && apiKey.Length > 64) ||
            //(!string.IsNullOrEmpty(apiKey) && apiKey.Length < 20))
            return false;
        
        return managerServiceProvider.IsKeyValid(apiKey);
    }
    
    public bool IsValidTrustApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) ) //|| 
            //(!string.IsNullOrEmpty(apiKey) && apiKey.Length > 64) ||
            //(!string.IsNullOrEmpty(apiKey) && apiKey.Length < 20))
            return false;
        
        return managerServiceProvider.IsDefaultKeyValid(apiKey);
    }
    
    public bool IsNonceValid(long nonce)
    {
        var diff = DateTime.UtcNow.Ticks - nonce;
        
        if(diff > 0 && diff/TimeSpan.TicksPerSecond <= 5) // less than 5 seconds.
            return true;
        
        return false;
    }
    
    private bool IsSecretApiKeyValid(string realSecretApiKey) => secretService.SecretApi.Equals(realSecretApiKey);
    
    public bool IsValid(string apiKey, long nonce, string realSecretApiKey) => IsValidApiKey(apiKey) 
                                                                               && IsNonceValid(nonce) 
                                                                               && IsSecretApiKeyValid(realSecretApiKey);
    
    public bool IsTrustValid(string apiKey, long nonce, string realSecretApiKey) => IsValidTrustApiKey(apiKey) 
                                                                               && IsNonceValid(nonce) 
                                                                               && IsSecretApiKeyValid(realSecretApiKey);
    
/*public bool IsValid(string apiKey, long nonce, string realSecretApiKey)
{
    var t= IsValidApiKey(apiKey);
    Console.WriteLine($"ApiKey Valid - {t}");
    var t1 = IsNonceValid(nonce);
    Console.WriteLine($"Nonce Valid - {t1}");
    var t2 = IsSecretApiKeyValid(realSecretApiKey);
    Console.WriteLine($"SecretKey Valid - {t2}");
    return t && t1 && t2;
}*/

   /* public bool IsTrustValid(string apiKey, long nonce, string realSecretApiKey)
    {
        Console.WriteLine($"ApiKey  - {apiKey}");
        var t1 = IsValidTrustApiKey(apiKey);
        Console.WriteLine($"ApiKey Valid - {t1}");
            //&& IsNonceValid(nonce) 
        Console.WriteLine($"RealSecretApiKey  - {realSecretApiKey}");
        var t2 = IsSecretApiKeyValid(realSecretApiKey);
        Console.WriteLine($"IsSecretApiKeyValid  - {t2}");
        return t1 && t2;
    } */

}