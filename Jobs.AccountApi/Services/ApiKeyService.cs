using Jobs.AccountApi.Contracts;
using Jobs.Common.Contracts;
using Jobs.Common.DataModel;
using Jobs.Common.Helpers;

namespace Jobs.AccountApi.Services;

public class ApiKeyService(IApiKeyStorageServiceProvider repository, ISecretApiService secretService) : IApiKeyService
{
    public async Task<ApiKey> GenerateApiKeyAsync()
    {
        var apiKey = new ApiKey
        {
            Key = ApiKeyGeneratorHelper.GenerateApiKey(),
            Expiration = DateTime.UtcNow.AddMinutes(30)
        };
        
        var result = await repository.AddApiKeyAsync(apiKey);

        return apiKey;
    }

    public bool IsValidApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) ) //|| 
            //(!string.IsNullOrEmpty(apiKey) && apiKey.Length > 64) ||
            //(!string.IsNullOrEmpty(apiKey) && apiKey.Length < 20))
            return false;
        
        return repository.IsKeyValid(apiKey);
    }
    
    public bool IsNonceValid(long nonce)
    {
        var diff = DateTime.UtcNow.Ticks - nonce;
        
        if(diff/TimeSpan.TicksPerSecond <= 5) // less than 5 seconds.
            return true;
        
        return false;
    }
    
    private bool IsSecretApiKeyValid(string realSecretApiKey) => secretService.SecretApi.Equals(realSecretApiKey);
    
    public bool IsValid(string apiKey, long nonce, string realSecretApiKey) => IsValidApiKey(apiKey) 
                                                                               && IsNonceValid(nonce) 
                                                                               && IsSecretApiKeyValid(realSecretApiKey);
    
}