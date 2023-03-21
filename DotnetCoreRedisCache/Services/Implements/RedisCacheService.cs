using DotnetCoreRedisCache.Infrastructure.Config;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace DotnetCoreRedisCache.Services.Implements
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabaseAsync _cache;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redisCache;

        public RedisCacheService(IConfiguration configuration, IConnectionMultiplexer redisCache)
        {
            _configuration = configuration;
            _redisCache = redisCache;
            _cache = redisCache.GetDatabase();
        }

        public async Task<bool> DeleteDataAsync<T>(string key)
        {
            var isKeyExist = await _cache.KeyExistsAsync(key);
            if(!isKeyExist)
                return false;
            return await _cache.KeyDeleteAsync(key);
        }

        public async Task<T> GetDataAsync<T>(string key)
        {
            var cacheData = await _cache.StringGetAsync(key);
            if (!cacheData.HasValue) return default;

            return JsonConvert.DeserializeObject<T>(cacheData);
        }

        public async Task<bool> SetDataAsync<T>(string key, T value, DateTimeOffset expirationTime)
        {
            var expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);
            var isSet = await _cache.StringSetAsync(key, JsonConvert.SerializeObject(value), expiryTime);

            return isSet;
        }

        public async Task<bool> SetDataAsync<T>(string key, T value)
        {
            var redisSetting = _configuration.GetSection("Redis").Get<RedisSetting>();
            var expirationTime = TimeSpan.FromSeconds(redisSetting.AbsoluteExpirationRelativeToNow);

            var isSet = await _cache.StringSetAsync(key, JsonConvert.SerializeObject(value), expirationTime);

            return isSet;
        }
    }
}
