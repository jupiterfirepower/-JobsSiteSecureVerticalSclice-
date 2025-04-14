using Jobs.Entities.Models;
using Jobs.VacancyApi.Contracts;
using Jobs.VacancyApi.Data;
using Microsoft.EntityFrameworkCore;

namespace Jobs.VacancyApi.Repository;

public class SecretApiKeyRepository(JobsDbContext context) : ISecretApiKeyRepository
{
    public async Task<SecretApiKey> GetCurrentSecretApiKey()
    {
        return await context.ApiKeys.OrderBy(x => x.Created).LastOrDefaultAsync();
    }
}