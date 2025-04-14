using Jobs.Common.DataModel;

namespace Jobs.Common.Contracts;

public interface IApiKeyStorageServiceProvider
{
    bool IsKeyValid(string key);
    void AddApiKey(ApiKey key);
    Task<bool> IsKeyValidAsync(string key);
    Task<bool> AddApiKeyAsync(ApiKey key);
    Task<int> DeleteExpiredKeysAsync();
}