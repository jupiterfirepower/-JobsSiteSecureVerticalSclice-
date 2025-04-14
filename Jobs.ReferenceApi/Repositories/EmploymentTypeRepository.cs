using System.Linq.Expressions;
using Jobs.Common.Contracts;
using Jobs.Common.Options;
using Jobs.Entities.Models;
using Jobs.ReferenceApi.Data;
using Microsoft.EntityFrameworkCore;

namespace Jobs.ReferenceApi.Repositories;

public class EmploymentTypeRepository(JobsDbContext context): IGenericRepository<EmploymentType>, IDisposable
{
    public IQueryable<EmploymentType> GetAllAsQueryable(FindOptions findOptions = null)
    {
        return context.EmploymentTypes.AsQueryable();
    }

    public async Task<EmploymentType> FindOneAsync(Expression<Func<EmploymentType, bool>> predicate, FindOptions findOptions = null)
    {
        return await context.EmploymentTypes.FirstOrDefaultAsync(predicate);
    }
    
    public IQueryable<EmploymentType> FindAsQueryable(Expression<Func<EmploymentType, bool>> predicate, FindOptions findOptions = null)
    {
        return context.EmploymentTypes.Where(predicate).AsQueryable();
    }

    public IEnumerable<EmploymentType> GetAll()
    {
        return context.EmploymentTypes.ToList();
    }

    public Task<List<EmploymentType>> GetAllAsync()
    {
        return context.EmploymentTypes.ToListAsync();
    }

    public EmploymentType GetById(int id)
    {
        return context.EmploymentTypes.Find(id)!;
    }

    public EmploymentType GetByIdWithIncludes(int id)
    {
        return context.EmploymentTypes.Find(id)!;
    }

    public async Task<EmploymentType> GetByIdAsync(int id)
    {
        return await context.EmploymentTypes.FindAsync(id);
    }

    public async Task<EmploymentType> GetByIdWithIncludesAsync(int id)
    {
        return await context.EmploymentTypes.FindAsync(id);
    }

    public bool Remove(int id)
    {
        var product = context.EmploymentTypes.Find(id);
        
        if (product is { })
        {
            context.EmploymentTypes.Remove(product);
            return true;
        }

        return false;
    }

    public void Add(in EmploymentType sender)
    {
        context.Add(sender).State = EntityState.Added;
    }

    public void Update(in EmploymentType sender)
    {
        context.Entry(sender).State = EntityState.Modified;
    }
    
    public void Change(EmploymentType currentVacancy, EmploymentType sender)
    {
        context.Entry(currentVacancy).CurrentValues.SetValues(sender);
    }

    public int Save()
    {
        return context.SaveChanges();
    }

    public Task<int> SaveAsync()
    {
        return context.SaveChangesAsync();
    }

    public EmploymentType Select(
        Expression<Func<EmploymentType, bool>> predicate)
    {
        return context.EmploymentTypes.FirstOrDefault(predicate)!;
    }

    public async Task<EmploymentType> SelectAsync(
        Expression<Func<EmploymentType, bool>> predicate)
    {
        return (await context.EmploymentTypes.
            FirstOrDefaultAsync(predicate))!;
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