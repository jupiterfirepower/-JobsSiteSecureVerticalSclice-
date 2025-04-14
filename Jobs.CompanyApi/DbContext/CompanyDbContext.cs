using Jobs.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Jobs.CompanyApi.DbContext;

public class CompanyDbContext(DbContextOptions<CompanyDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<SecretApiKey> ApiKeys => Set<SecretApiKey>();
    public DbSet<Company> Companies => Set<Company>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}