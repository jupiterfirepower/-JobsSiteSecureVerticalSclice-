using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;

namespace Jobs.Core.Providers;

public class RockDbApiKeyStorageServiceProvider(ApiKeyRockDbStore store): IApiKeyStorageServiceProvider, IDisposable, IAsyncDisposable
{
    public bool IsKeyValid(string key)
    {
        if (store.HasKey(key))
        {
            store.TryGet(key, out var apiKey);
            
            if (apiKey != null && apiKey.Expiration >= DateTime.UtcNow)
            {
                return true;
            }
        }

        return false;
    }
    
    public bool IsDefaultKeyValid(string key)
    {
        if (store.HasKey(key))
        {
            store.TryGet(key, out var apiKey);
            
            if (apiKey is { Expiration: null })
            {
                return true;
            }
        }

        return false;
    }
    
    public async Task<bool> IsKeyValidAsync(string key)
    {
        var result = IsKeyValid(key);
        return await Task.FromResult(result);
    }
    public void AddApiKey(ApiKey akey)
    {
        store.Put(akey.Key, akey);
    }
    
    public async Task<bool> AddApiKeyAsync(ApiKey akey)
    {
        AddApiKey(akey);
        return await Task.FromResult(true);
    }
    
    public async Task<int> DeleteExpiredKeysAsync()
    {
        int count = 0;

        foreach (var current in store.GetAll())
        {
            store.TryGet(current.Key, out var apiKey);
            
            if (apiKey != null && apiKey.Expiration <= DateTime.UtcNow)
            {
                store.Remove(current.Key);
                count++;
            }
        }

        return await Task.FromResult(count);
    }
    
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                ClearApiKeyStore();
            }
        }
        _disposed = true;
    }

    private void ClearApiKeyStore()
    {
        foreach (var current in store.GetAll())
        {
            store.Remove(current.Key);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync() => await DisposeAsync(true);
    
    private async ValueTask DisposeAsync(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                ClearApiKeyStore();
                await Task.CompletedTask;
                return;
            }
        }
        _disposed = true;
    }
}