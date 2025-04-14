using Jobs.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Jobs.ReferenceApi.Data;

public class JobsDbContext(DbContextOptions<JobsDbContext>
    options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    
    public DbSet<WorkType> WorkTypes => Set<WorkType>();
    
    public DbSet<EmploymentType> EmploymentTypes => Set<EmploymentType>();
    
    public DbSet<SecretApiKey> ApiKeys => Set<SecretApiKey>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Seed();
    }
}
