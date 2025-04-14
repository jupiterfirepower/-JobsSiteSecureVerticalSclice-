using Jobs.Common.DataModel;

namespace Jobs.AccountApi.Contracts;

public interface IApiKeyService
{
    Task<ApiKey> GenerateApiKeyAsync();
    bool IsValidApiKey(string apiKey);
    bool IsNonceValid(long nonce);
    bool IsValid(string apiKey, long nonce, string realSecretApiKey);
}