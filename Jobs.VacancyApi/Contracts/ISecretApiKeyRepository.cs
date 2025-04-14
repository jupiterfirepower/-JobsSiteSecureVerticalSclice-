using Jobs.Entities.Models;

namespace Jobs.VacancyApi.Contracts;

public interface ISecretApiKeyRepository
{
    Task<SecretApiKey> GetCurrentSecretApiKey();
}