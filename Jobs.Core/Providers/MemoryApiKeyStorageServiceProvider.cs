using Jobs.Core.Contracts.Providers;
using Jobs.Core.Contracts.Services;
using Jobs.Core.DataModel;
using Jobs.Core.Services;

namespace Jobs.Core.Providers;

public class MemoryApiKeyStorageServiceProvider : IApiKeyStorageServiceProvider
{
    private readonly IConcurrentStorageService<ApiKey> _cache = ConcurrentStorageService<ApiKey>.GetInstance();
    
    public bool IsKeyValid(string key)
    {
        var apiKey = _cache.Get(key);
        
        _cache.GetAll().
            ToList().
            ForEach(x=>
            {
                Console.WriteLine($"MemoryApiKeyStorageServiceProvider Key : {x.Key}");
            });
        
        if (apiKey == null)
            return false;
        
        /*if (!apiKey.Expiration.HasValue)
        {
            return true;
        }*/

        if (apiKey.Expiration.HasValue && apiKey.Expiration >= DateTime.UtcNow)
        {
            _cache.Remove(key);
            return true;
        }

        return false;
    }
    
    public bool IsDefaultKeyValid(string key)
    {
        var apiKey = _cache.Get(key);
        
        if (apiKey == null)
            return false;
        
        if (!apiKey.Expiration.HasValue)
        {
            return true;
        }

        return false;
    }

    public void AddApiKey(ApiKey key) => _cache.Add(key.Key, key);


    public async Task<bool> IsKeyValidAsync(string key)
    {
        return await Task.Run(() => IsKeyValid(key));
    }

    public async Task<bool> AddApiKeyAsync(ApiKey key)
    {
        return await Task.Run(() => { 
            AddApiKey(key);
            return true;
        });
    }

    public async Task<int> DeleteExpiredKeysAsync()
    {
        return await Task.Run(() => { 
            int count = 0;
            
            var data = _cache.GetAll()
                                          .Where(x=> x.Expiration.HasValue && x.Expiration.Value < DateTime.UtcNow);
            // Optimized by performance
            foreach (var current in data)
            {
                _cache.Remove(current.Key);
                count++;
            }    
            /*
            _cache.GetAll().
                Where(x=> x.Expiration.HasValue && x.Expiration.Value < DateTime.UtcNow)
                .ToList()
                .ForEach(x=>
                {
                    _cache.Remove(x.Key);
                    count++;
                });*/
            
            return count;
        });
    }
}