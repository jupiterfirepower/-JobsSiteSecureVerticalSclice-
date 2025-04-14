using System.Linq.Expressions;
using Jobs.Common.Contracts;
using Jobs.Common.Options;
using Jobs.Entities.Models;
using Jobs.VacancyApi.Data;
using Microsoft.EntityFrameworkCore;

namespace Jobs.VacancyApi.Repository;

public sealed class VacancyRepository(JobsDbContext context) : IGenericRepository<Vacancy>, IDisposable
{
    public IQueryable<Vacancy> GetAllAsQueryable(FindOptions findOptions = null)
    {
        return context.Vacancies.AsQueryable();
    }

    public async Task<Vacancy> FindOneAsync(Expression<Func<Vacancy, bool>> predicate, FindOptions findOptions = null)
    {
        return await context.Vacancies.FirstOrDefaultAsync(predicate);
    }
    
    public IQueryable<Vacancy> FindAsQueryable(Expression<Func<Vacancy, bool>> predicate, FindOptions findOptions = null)
    {
        return context.Vacancies.Where(predicate).AsQueryable();
    }
    
    public IEnumerable<Vacancy> GetAll()
    {
        return context.Vacancies.ToList();
    }

    public Task<List<Vacancy>> GetAllAsync()
    {
        return context.Vacancies.ToListAsync();
    }

    public Vacancy GetById(int id)
    {
        return context.Vacancies.Find(id)!;
    }

    public Vacancy GetByIdWithIncludes(int id)
    {
        return context.Vacancies
            .Include(x => x.Category)
            .ThenInclude(x=>x.ParentCategory)
            .FirstOrDefault(x => x.VacancyId == id)!;
    }

    public async Task<Vacancy> GetByIdAsync(int id)
    {
        return await context.Vacancies.FindAsync(id);
    }

    public async Task<Vacancy> GetByIdWithIncludesAsync(int id)
    {
        return await context.Vacancies
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.VacancyId == id);
    }

    public bool Remove(int id)
    {
        var product = context.Vacancies.Find(id);
        
        if (product is { })
        {
            context.Vacancies.Remove(product);
            return true;
        }

        return false;
    }

    public void Add(in Vacancy sender)
    {
        context.Add(sender).State = EntityState.Added;
    }

    public void Update(in Vacancy sender)
    {
        context.Entry(sender).State = EntityState.Modified;
    }
    
    public void Change(Vacancy currentVacancy, Vacancy sender)
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

    public Vacancy Select(
        Expression<Func<Vacancy, bool>> predicate)
    {
        return context.Vacancies.FirstOrDefault(predicate)!;
    }

    public async Task<Vacancy> SelectAsync(
        Expression<Func<Vacancy, bool>> predicate)
    {
        return (await context.Vacancies
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