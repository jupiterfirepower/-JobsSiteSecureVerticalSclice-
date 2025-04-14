using System.Linq.Expressions;
using Jobs.Common.Contracts;
using Jobs.Common.Options;
using Jobs.Entities.Models;
using Jobs.ReferenceApi.Data;
using Microsoft.EntityFrameworkCore;

namespace Jobs.ReferenceApi.Repositories;

public class CategoryRepository(JobsDbContext context) : IGenericRepository<Category>, IDisposable
{
    public IQueryable<Category> GetAllAsQueryable(FindOptions findOptions = null)
    {
        return context.Categories.AsQueryable();
    }

    public async Task<Category> FindOneAsync(Expression<Func<Category, bool>> predicate, FindOptions findOptions = null)
    {
        return await context.Categories.FirstOrDefaultAsync(predicate);
    }
    
    public IQueryable<Category> FindAsQueryable(Expression<Func<Category, bool>> predicate, FindOptions findOptions = null)
    {
        return context.Categories.Where(predicate).AsQueryable();
    }

    public IEnumerable<Category> GetAll()
    {
        return context.Categories.ToList();
    }

    public Task<List<Category>> GetAllAsync()
    {
        return context.Categories.ToListAsync();
    }

    public Category GetById(int id)
    {
        return context.Categories.Find(id)!;
    }

    public Category GetByIdWithIncludes(int id)
    {
        return context.Categories.Find(id)!;
    }

    public async Task<Category> GetByIdAsync(int id)
    {
        return await context.Categories.FindAsync(id);
    }

    public async Task<Category> GetByIdWithIncludesAsync(int id)
    {
        return await context.Categories.FindAsync(id);
    }

    public bool Remove(int id)
    {
        var product = context.Categories.Find(id);
        
        if (product is { })
        {
            context.Categories.Remove(product);
            return true;
        }

        return false;
    }

    public void Add(in Category sender)
    {
        context.Add(sender).State = EntityState.Added;
    }

    public void Update(in Category sender)
    {
        context.Entry(sender).State = EntityState.Modified;
    }
    
    public void Change(Category currentVacancy, Category sender)
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

    public Category Select(
        Expression<Func<Category, bool>> predicate)
    {
        return context.Categories.FirstOrDefault(predicate)!;
    }

    public async Task<Category> SelectAsync(
        Expression<Func<Category, bool>> predicate)
    {
        return (await context.Categories.
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