using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;

namespace Jobs.Core.Providers;

public class RockDbApiKeyStorageServiceProvider(ApiKeyRockDbStore store): IApiKeyStorageServiceProvider, IDisposable, IAsyncDisposable
{
    private readonly ApiKeyRockDbStore _apiKeyStore = store;
    
    public bool IsKeyValid(string key)
    {
        if (_apiKeyStore.HasKey(key))
        {
            _apiKeyStore.TryGet(key, out var apiKey);
            
            if (apiKey != null && apiKey.Expiration >= DateTime.UtcNow)
            {
                return true;
            }
        }

        return false;
    }
    
    public bool IsDefaultKeyValid(string key)
    {
        if (_apiKeyStore.HasKey(key))
        {
            _apiKeyStore.TryGet(key, out var apiKey);
            
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
        _apiKeyStore.Put(akey.Key, akey);
    }
    
    public async Task<bool> AddApiKeyAsync(ApiKey akey)
    {
        AddApiKey(akey);
        return await Task.FromResult(true);
    }
    
    public async Task<int> DeleteExpiredKeysAsync()
    {
        int count = 0;
        _apiKeyStore.GetAll().ToList().ForEach(x=>
        {
            _apiKeyStore.TryGet(x.Key, out var apiKey);
            
            if (apiKey != null && apiKey.Expiration <= DateTime.Now)
            {
                _apiKeyStore.Remove(x.Key);
                count++;
            }
            
        });
        return await Task.FromResult(count);
    }
    
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _apiKeyStore.GetAll().ToList().ForEach(x=>_apiKeyStore.Remove(x.Key));
            }
        }
        _disposed = true;
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
                _apiKeyStore.GetAll().ToList().ForEach(x=>_apiKeyStore.Remove(x.Key));
                await Task.CompletedTask;
                return;
            }
        }
        _disposed = true;
    }
}