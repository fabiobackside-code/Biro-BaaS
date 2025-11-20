using System.Text.Json;
using StackExchange.Redis;

namespace Biro.Infrastructure.Cache;

public class RedisCacheProvider : ICacheProvider
{
    private readonly IDatabase _database;

    public RedisCacheProvider(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<T> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, serializedValue, expiry);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }
}
