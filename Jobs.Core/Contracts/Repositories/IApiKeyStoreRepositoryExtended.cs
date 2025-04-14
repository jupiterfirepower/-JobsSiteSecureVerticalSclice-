using Jobs.Core.DataModel;

namespace Jobs.Core.Contracts.Repositories;

public interface IApiKeyStoreRepositoryExtended : IApiKeyStoreRepository
{
    Task<List<ApiKey>> GetAllAsync();
}