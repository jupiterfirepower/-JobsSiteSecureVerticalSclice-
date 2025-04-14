using Jobs.Core.DataModel;

namespace Jobs.Core.Contracts.Providers;

public interface IApiKeyStorageServiceProvider
{
    bool IsKeyValid(string key);
    bool IsDefaultKeyValid(string key);
    void AddApiKey(ApiKey key);
    Task<bool> IsKeyValidAsync(string key);
    Task<bool> AddApiKeyAsync(ApiKey key);
    Task<int> DeleteExpiredKeysAsync();
}