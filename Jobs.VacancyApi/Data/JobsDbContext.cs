using Jobs.Entities.Models;
using Jobs.VacancyApi.Extentions;
using Microsoft.EntityFrameworkCore;

namespace Jobs.VacancyApi.Data;

public class JobsDbContext(DbContextOptions<JobsDbContext>
    options) : DbContext(options)
{
    /*public JobsDbContext(DbContextOptions<JobsDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }*/
    
    public DbSet<SecretApiKey> ApiKeys => Set<SecretApiKey>();
    
    public DbSet<Category> Categories => Set<Category>();
    
    public DbSet<WorkType> WorkTypes => Set<WorkType>();
    
    public DbSet<EmploymentType> EmploymentTypes => Set<EmploymentType>();
    
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();
    
    public DbSet<Company> Companies => Set<Company>();
    
    public DbSet<VacancyWorkTypes> VacancyWorkTypes => Set<VacancyWorkTypes>();
    
    public DbSet<VacancyEmploymentTypes> VacancyEmploymentTypes => Set<VacancyEmploymentTypes>();
    
    public DbSet<CompanyOwnerEmails> CompanyOwnerEmails => Set<CompanyOwnerEmails>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Seed();
    }
}
