using Jobs.Core.DataModel;

namespace Jobs.Core.Contracts.Repositories;

public interface IApiKeyStoreRepository
{
    void Add(ApiKey item);
    Task AddAsync(ApiKey item);
    ApiKey Get(string key);
    Task<ApiKey> GetAsync(string key);
    void Remove(string key);
    Task RemoveAsync(string key);
}