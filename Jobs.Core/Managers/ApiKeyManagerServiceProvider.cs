using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;

namespace Jobs.Core.Managers;

public class ApiKeyManagerServiceProvider(IApiKeyStorageServiceProvider backendProvider) : IApiKeyManagerServiceProvider
{
    public bool IsKeyValid(string key) => backendProvider.IsKeyValid(key);
    
    public bool IsDefaultKeyValid(string key) => backendProvider.IsDefaultKeyValid(key);

    public void AddApiKey(ApiKey key) => backendProvider.AddApiKey(key);

    public async Task<bool> IsKeyValidAsync(string key) => await backendProvider.IsKeyValidAsync(key);

    public async Task<bool> AddApiKeyAsync(ApiKey key) => await backendProvider.AddApiKeyAsync(key);

    public async Task<int> DeleteExpiredKeysAsync() => await backendProvider.DeleteExpiredKeysAsync();
}