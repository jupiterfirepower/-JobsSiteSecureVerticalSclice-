using System.Linq.Expressions;
using Jobs.Common.Options;

namespace Jobs.Common.Contracts;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetAllAsQueryable(FindOptions? findOptions = null);

    Task<T> FindOneAsync(Expression<Func<T, bool>> predicate, FindOptions findOptions = null);
    IQueryable<T> FindAsQueryable(Expression<Func<T, bool>> predicate, FindOptions findOptions = null);

    IEnumerable<T> GetAll();
    Task<List<T>> GetAllAsync();
    T GetById(int id);
    T GetByIdWithIncludes(int id);
    Task<T> GetByIdAsync(int id);
    Task<T> GetByIdWithIncludesAsync(int id);
    bool Remove(int id);
    void Add(in T sender);
    void Update(in T sender);
    void Change(T currentVacancy, T sender);
    int Save();
    Task<int> SaveAsync();
    public T Select(Expression<Func<T, bool>> predicate);
    public Task<T> SelectAsync(Expression<Func<T, bool>> predicate);
}