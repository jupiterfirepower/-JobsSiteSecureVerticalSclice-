using System.Linq.Expressions;
using Jobs.Common.Contracts;
using Jobs.Common.Options;
using Jobs.CompanyApi.DbContext;
using Jobs.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Jobs.CompanyApi.Repositories;

public sealed class CompanyRepository(CompanyDbContext context) : IGenericRepository<Company>, IDisposable
{
    public IQueryable<Company> GetAllAsQueryable(FindOptions? findOptions = null)
    {
        return  context.Companies.AsQueryable();
    }

    public async Task<Company> FindOneAsync(Expression<Func<Company, bool>> predicate, FindOptions? findOptions = null)
    {
        return await context.Companies.FirstOrDefaultAsync(predicate);
    }
    
    public IQueryable<Company> FindAsQueryable(Expression<Func<Company, bool>> predicate, FindOptions? findOptions = null)
    {
        return context.Companies.Where(predicate).AsQueryable();
    }
    
    public IEnumerable<Company> GetAll()
    {
        return context.Companies.ToList();
    }

    public async Task<List<Company>> GetAllAsync()
    {
        return await context.Companies.ToListAsync();
    }

    public Company GetById(int id)
    {
        return context.Companies.Find(id)!;
    }

    public Company GetByIdWithIncludes(int id)
    {
        throw new NotImplementedException();
        /*return context.Companies
            .Include(x => x.Category)
            .ThenInclude(x=>x.ParentCategory)
            .FirstOrDefault(x => x.VacancyId == id)!;*/
    }

    public async Task<Company> GetByIdAsync(int id)
    {
        return await context.Companies.FindAsync(id);
    }

    public async Task<Company> GetByIdWithIncludesAsync(int id)
    {
        throw new NotImplementedException();
        /*return await context.Companies
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.VacancyId == id);*/
    }

    public bool Remove(int id)
    {
        var product = context.Companies.Find(id);
        
        if (product is { })
        {
            context.Companies.Remove(product);
            return true;
        }

        return false;
    }

    public void Add(in Company sender)
    {
        context.Add(sender).State = EntityState.Added;
    }

    public void Update(in Company sender)
    {
        context.Entry(sender).State = EntityState.Modified;
    }
    
    public void Change(Company currentVacancy, Company sender)
    {
        context.Entry(currentVacancy).CurrentValues.SetValues(sender);
        currentVacancy.Modified = DateTime.UtcNow;
    }

    public int Save()
    {
        return context.SaveChanges();
    }

    public Task<int> SaveAsync()
    {
        return context.SaveChangesAsync();
    }

    public Company Select(
        Expression<Func<Company, bool>> predicate)
    {
        return context.Companies.FirstOrDefault(predicate)!;
    }

    public async Task<Company> SelectAsync(
        Expression<Func<Company, bool>> predicate)
    {
        return (await context.Companies
                .FirstOrDefaultAsync(predicate))!;
    }
    
    private bool _disposed = false;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                context.Dispose();
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}