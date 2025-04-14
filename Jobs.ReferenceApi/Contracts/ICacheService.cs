namespace Jobs.ReferenceApi.Contracts;

public interface ICacheService
{
    bool HasData(string key);
    T GetData<T>(string key);
    bool SetData<T>(string key, T value, DateTimeOffset expirationTime);
    object RemoveData(string key);
}