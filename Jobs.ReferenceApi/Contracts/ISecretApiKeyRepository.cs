using Jobs.Entities.Models;

namespace Jobs.ReferenceApi.Contracts;

public interface ISecretApiKeyRepository
{
    Task<SecretApiKey> GetCurrentSecretApiKey();
}