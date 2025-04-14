using Jobs.CompanyApi.DbContext;
using Jobs.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Jobs.CompanyApi.Repositories;

public class SecretApiKeyRepository(CompanyDbContext context) : ISecretApiKeyRepository
{
    public async Task<SecretApiKey> GetCurrentSecretApiKey()
    {
        return await context.ApiKeys.
            Where(x=> x.IsActive).
            OrderBy(x => x.Created).
            LastOrDefaultAsync();
    }
}