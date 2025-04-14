namespace Jobs.Common.Contracts;

public interface IApiKeyStorageService<T>
{
    bool Add(string key, T value);

    bool AddOrUpdate(string key, T value);

    T Get(string key);

    bool Remove(string key);

    List<T> GetAll();

    void Clear();
}