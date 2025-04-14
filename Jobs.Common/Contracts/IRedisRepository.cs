namespace Jobs.Common.Contracts;

public interface IRedisRepository
{
    Task SetValueAsync<T>(string key, T value, TimeSpan expiration);
    Task<T> GetValueAsync<T>(string key);
}