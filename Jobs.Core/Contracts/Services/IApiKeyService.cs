using Jobs.Core.DataModel;

namespace Jobs.Core.Contracts;

public interface IApiKeyService
{
    Task<ApiKey> GenerateApiKeyAsync();
    bool IsValidApiKey(string apiKey);
    bool IsValidTrustApiKey(string apiKey);
    bool IsNonceValid(long nonce);
    bool IsValid(string apiKey, long nonce, string realSecretApiKey);
    bool IsTrustValid(string apiKey, long nonce, string realSecretApiKey);
}