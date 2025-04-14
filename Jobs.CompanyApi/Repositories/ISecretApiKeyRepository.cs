using Jobs.Entities.Models;

namespace Jobs.CompanyApi.Repositories;

public interface ISecretApiKeyRepository
{
    Task<SecretApiKey> GetCurrentSecretApiKey();
}