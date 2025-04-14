using Jobs.Common.Contracts;
using Jobs.Common.DataModel;

namespace Jobs.Common.Repository;

public class MemLiteRepository : ILiteDbRepository
{
    private readonly CacheService<ApiKey> _cache = CacheService<ApiKey>.GetInstance();
    
    public bool IsKeyValid(string key)
    {
        var result = IsKeyValidAsync(key);
        return result.Result;
    }

    public void AddApiKey(ApiKey key)
    {
        _cache.Add(key.Key, key);
    }

    public async Task<bool> IsKeyValidAsync(string key)
    {
        var apiKey = _cache.Get(key);
        
        if (apiKey == null)
            return await Task.FromResult(false);

        if (apiKey.Expiration >= DateTime.UtcNow)
        {
            _cache.Remove(key);
            return await Task.FromResult(true);
        }

        return await Task.FromResult(false);
    }

    public async Task<bool> AddApiKeyAsync(ApiKey key)
    {
        _cache.Add(key.Key, key);
        return await Task.FromResult(true);
    }

    public async Task<int> DeleteExpiredKeysAsync()
    {
        return await Task.FromResult(0);
    }
}