using Jobs.Common.Contracts;
using Jobs.Common.DataModel;
using Jobs.Common.Repository;

namespace Jobs.Common.Providers;

public class MemoryApiKeyStorageServiceProvider : IApiKeyStorageServiceProvider
{
    private readonly IApiKeyStorageService<ApiKey> _cache = ApiKeyStorageService<ApiKey>.GetInstance();
    
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
        
        if (!apiKey.Expiration.HasValue)
        {
            return await Task.FromResult(true);
        }

        if (apiKey.Expiration.HasValue && apiKey.Expiration >= DateTime.UtcNow)
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
        int count = 0;
        
        _cache.GetAll().
            Where(x=> x.Expiration.HasValue && x.Expiration.Value < DateTime.UtcNow).
            ToList().
            ForEach(x=>
            {
                _cache.Remove(x.Key);
                count++;
            });

        return await Task.FromResult(count);
    }
}