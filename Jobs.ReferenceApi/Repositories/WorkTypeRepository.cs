using System.Linq.Expressions;
using Jobs.Common.Contracts;
using Jobs.Common.Options;
using Jobs.Entities.Models;
using Jobs.ReferenceApi.Data;
using Microsoft.EntityFrameworkCore;

namespace Jobs.ReferenceApi.Repositories;

public sealed class WorkTypeRepository(JobsDbContext context): IGenericRepository<WorkType>, IDisposable
{
    public IQueryable<WorkType> GetAllAsQueryable(FindOptions findOptions = null)
    {
        return context.WorkTypes.AsQueryable();
    }

    public async Task<WorkType> FindOneAsync(Expression<Func<WorkType, bool>> predicate, FindOptions findOptions = null)
    {
        return await context.WorkTypes.FirstOrDefaultAsync(predicate);
    }

    public IQueryable<WorkType> FindAsQueryable(Expression<Func<WorkType, bool>> predicate, FindOptions findOptions = null)
    {
        return context.WorkTypes.Where(predicate).AsQueryable();
    }

    public IEnumerable<WorkType> GetAll()
    {
        return context.WorkTypes.ToList();
    }

    public Task<List<WorkType>> GetAllAsync()
    {
        return context.WorkTypes.ToListAsync();
    }

    public WorkType GetById(int id)
    {
        return context.WorkTypes.Find(id)!;
    }

    public WorkType GetByIdWithIncludes(int id)
    {
        return context.WorkTypes.Find(id)!;
    }

    public async Task<WorkType> GetByIdAsync(int id)
    {
        return await context.WorkTypes.FindAsync(id);
    }

    public async Task<WorkType> GetByIdWithIncludesAsync(int id)
    {
        return await context.WorkTypes.FindAsync(id);
    }

    public bool Remove(int id)
    {
        var product = context.WorkTypes.Find(id);
        
        if (product is { })
        {
            context.WorkTypes.Remove(product);
            return true;
        }

        return false;
    }

    public void Add(in WorkType sender)
    {
        context.Add(sender).State = EntityState.Added;
    }

    public void Update(in WorkType sender)
    {
        context.Entry(sender).State = EntityState.Modified;
    }
    
    public void Change(WorkType currentVacancy, WorkType sender)
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

    public WorkType Select(
        Expression<Func<WorkType, bool>> predicate)
    {
        return context.WorkTypes.FirstOrDefault(predicate)!;
    }

    public async Task<WorkType> SelectAsync(
        Expression<Func<WorkType, bool>> predicate)
    {
        return (await context.WorkTypes
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