using System.Text.Json;
using Jobs.Common.Contracts;
using StackExchange.Redis;

namespace Jobs.Common.Repository;

public class RedisDbRepository(IConnectionMultiplexer redis) : IRedisRepository
{
    public async Task SetValueAsync<T>(string key, T value, TimeSpan expiration)
    {
        var db = redis.GetDatabase();
        var json = JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, json, expiration);
    }

    public async Task<T> GetValueAsync<T>(string key)
    {
        var db = redis.GetDatabase();
        var json = await db.StringGetAsync(key);
        return json.HasValue ? JsonSerializer.Deserialize<T>(json) : default;
    }
}