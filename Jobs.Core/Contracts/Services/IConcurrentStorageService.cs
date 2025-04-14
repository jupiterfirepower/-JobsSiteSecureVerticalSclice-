namespace Jobs.Core.Contracts.Services;

public interface IConcurrentStorageService<T>
{
    bool Add(string key, T value);

    bool AddOrUpdate(string key, T value);

    T Get(string key);

    bool Remove(string key);

    List<T> GetAll();

    void Clear();
}