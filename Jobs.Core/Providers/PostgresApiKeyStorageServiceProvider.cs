using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.Contracts.Repositories;
using Jobs.Core.DataModel;

namespace Jobs.Core.Providers;

public class PostgresApiKeyStorageServiceProvider(IApiKeyStoreRepositoryExtended repository) : IApiKeyStorageServiceProvider
{
    public bool IsKeyValid(string key)
    {
        var current = repository.Get(key);
        
        if (current != null && current.Expiration >= DateTime.UtcNow)
        {
            return true;
        }

        return false;
    }
    
    public bool IsDefaultKeyValid(string key)
    {
        var current = repository.Get(key);
        
        if (current != null && !current.Expiration.HasValue)
        {
            return true;
        }

        return false;
    }

    public void AddApiKey(ApiKey item)
    {
        repository.Add(item);
    }

    public async Task<bool> IsKeyValidAsync(string key)
    {
        var current = await repository.GetAsync(key);
        
        if (current != null && current.Expiration >= DateTime.UtcNow)
        {
            return true;
        }

        return false;
    }

    public async Task<bool> AddApiKeyAsync(ApiKey item)
    {
        await repository.AddAsync(item);
        return true;
    }

    public async Task<int> DeleteExpiredKeysAsync()
    {
        return await Task.Run(async () => { 
            int count = 0;
            var data = await repository.GetAllAsync().ConfigureAwait(false);
            
            data.Where(x=> x.Expiration.HasValue && x.Expiration.Value < DateTime.UtcNow).
                ToList().
                ForEach(x=>
                {
                    repository.Remove(x.Key);
                    count++;
                });
            
            return count;
        });
    }
}