using System.Linq.Expressions;
using Jobs.Common.Contracts;
using Jobs.Common.Options;

namespace Jobs.Common.Repository;

public class GenericRepository<T>() : IGenericRepository<T> where T : class
{
    public IQueryable<T> GetAllAsQueryable(FindOptions findOptions = null)
    {
        throw new NotImplementedException();
    }

    public Task<T> FindOneAsync(Expression<Func<T, bool>> predicate, FindOptions findOptions = null)
    {
        throw new NotImplementedException();
    }

    public IQueryable<T> FindAsQueryable(Expression<Func<T, bool>> predicate, FindOptions findOptions = null)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<T> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public T GetById(int id)
    {
        throw new NotImplementedException();
    }

    public T GetByIdWithIncludes(int id)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetByIdWithIncludesAsync(int id)
    {
        throw new NotImplementedException();
    }

    public bool Remove(int id)
    {
        throw new NotImplementedException();
    }

    public void Add(in T sender)
    {
        throw new NotImplementedException();
    }

    public void Update(in T sender)
    {
        throw new NotImplementedException();
    }

    public void Change(T currentVacancy, T sender)
    {
        throw new NotImplementedException();
    }

    public int Save()
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveAsync()
    {
        throw new NotImplementedException();
    }

    public T Select(Expression<Func<T, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<T> SelectAsync(Expression<Func<T, bool>> predicate)
    {
        throw new NotImplementedException();
    }
}