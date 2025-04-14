using Jobs.Core.Contracts.Providers;
using Jobs.Core.Contracts.Repositories;
using Jobs.Core.DataModel;

namespace Jobs.Core.Providers;

public class ArangoDbApiKeyStorageServiceProvider(IApiKeyStoreRepositoryExtended repository)
    : IApiKeyStorageServiceProvider
{
    public bool IsKeyValid(string key)
    {
        var task = Task.Run(async () => await IsKeyValidAsync(key));
        return task.Result;
    }

    public void AddApiKey(ApiKey key)
    {
        Task.Run(async () => await AddApiKeyAsync(key));
    }

    public async Task<bool> IsKeyValidAsync(string key)
    {
        var current = await repository.GetAsync(key);

        if (current == null)
            return false;

        if (!current.Expiration.HasValue)
        {
            return true;
        }

        if (current.Expiration.HasValue && current.Expiration >= DateTime.UtcNow)
        {
            await repository.RemoveAsync(key);
            return true;
        }

        return false;
    }

    public bool IsDefaultKeyValid(string key)
    {
        var task = Task.Run(async () => await IsDefaultKeyValidAsync(key));
        return task.Result;
    }

    private async Task<bool> IsDefaultKeyValidAsync(string key)
    {
        var current = await repository.GetAsync(key);
        
        if (current == null)
            return false;
        
        if (!current.Expiration.HasValue)
        {
            return true;
        }

        return false;
    }

    public async Task<bool> AddApiKeyAsync(ApiKey key)
    {
        await repository.AddAsync(key);
        return true;
    }

    public async Task<int> DeleteExpiredKeysAsync()
    {
        return await Task.Run(async () => { 
            int count = 0;
            var data = await repository.GetAllAsync();
        
            data.
                Where(x=> x.Expiration.HasValue && x.Expiration.Value < DateTime.UtcNow).
                ToList().
                ForEach(x=>
                {
                    //repository.Remove(x.Key);
                    var task = Task.Run(async () => await repository.RemoveAsync(x.Key));
                    task.Wait();
                    count++;
                });
            
            return count;
        });
    }
}