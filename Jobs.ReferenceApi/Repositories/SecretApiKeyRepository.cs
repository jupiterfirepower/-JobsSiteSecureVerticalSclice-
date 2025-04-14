using Jobs.Entities.Models;
using Jobs.ReferenceApi.Contracts;
using Jobs.ReferenceApi.Data;
using Microsoft.EntityFrameworkCore;

namespace Jobs.ReferenceApi.Repositories;

public class SecretApiKeyRepository(JobsDbContext context) : ISecretApiKeyRepository
{
    public async Task<SecretApiKey> GetCurrentSecretApiKey()
    {
        return await context.ApiKeys.OrderBy(x => x.Created).LastOrDefaultAsync();
    }
}