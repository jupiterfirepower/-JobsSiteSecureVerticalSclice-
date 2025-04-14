using Jobs.ReferenceApi.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace Jobs.ReferenceApi.Services;

public class LocalCacheService(IMemoryCache memoryCache) : ICacheService
{
    public bool HasData(string key) => !string.IsNullOrEmpty(key) && memoryCache.TryGetValue(key, out var data);
    
    public T GetData<T>(string key)
    {
        T item = (T)memoryCache.Get(key)!;
        return item;
    }

    public object RemoveData(string key)
    {
        var res = true;
        
        if (!string.IsNullOrEmpty(key))
        {
            memoryCache.Remove(key);
        }
        else
        {
            res = false;
        }
        
        return res;
    }

    public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
    {
        var res = true;
        
        if (!string.IsNullOrEmpty(key))
        {
            memoryCache.Set(key, value, expirationTime);
        }
        else
        {
            res = false;
        }
        
        return res;
    }
}