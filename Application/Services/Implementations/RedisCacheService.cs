using Application.Services.Contracts;
using StackExchange.Redis;
using System.Text.Json;

namespace Application.Services.Implementations
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        private string BuildKey(string tenantId, string key)
        {
            return $"tenant:{tenantId}:{key}";
        }

        public async Task SetAsync<T>(string tenantId, string key, T value, TimeSpan? expiry = null)
        {
            var fullKey = BuildKey(tenantId, key);
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(fullKey, json, expiry);
        }

        public async Task<T?> GetAsync<T>(string tenantId, string key)
        {
            var fullKey = BuildKey(tenantId, key);
            var value = await _db.StringGetAsync(fullKey);
            if (value.IsNullOrEmpty) return default;
            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task RemoveAsync(string tenantId, string key)
        {
            var fullKey = BuildKey(tenantId, key);
            await _db.KeyDeleteAsync(fullKey);
        }
    }
}
