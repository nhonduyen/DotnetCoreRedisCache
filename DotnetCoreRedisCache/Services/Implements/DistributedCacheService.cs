using DotnetCoreRedisCache.Infrastructure.Config;
using DotnetCoreRedisCache.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace DotnetCoreRedisCache.Services.Implements
{
    public class DistributedCacheService : IDistributedCacheService
    {
        public readonly IConfiguration _configuration;
        public readonly IDistributedCache _cache;

         public DistributedCacheService(IConfiguration configuration, IDistributedCache distributedCacheService)
        {
            _configuration = configuration;
            _cache = distributedCacheService;
        }

        public async Task DeleteDataAsync<T>(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task<T> GetDataAsync<T>(string key)
        {
            var cacheData = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cacheData)) return default;

            return JsonConvert.DeserializeObject<T>(cacheData);
        }

        public async Task SetDataAsync<T>(string key, T value, DateTimeOffset expirationTime)
        {
            var redisSetting = _configuration.GetSection("Redis").Get<RedisSetting>();
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expirationTime
            };

            await _cache.SetStringAsync(key, JsonConvert.SerializeObject(value), options);
        }

        public async Task SetDataAsync<T>(string key, T value)
        {
            var redisSetting = _configuration.GetSection("Redis").Get<RedisSetting>();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(redisSetting.AbsoluteExpirationRelativeToNow),
                SlidingExpiration = TimeSpan.FromSeconds(redisSetting.SlidingExpiration)
            };

            await _cache.SetStringAsync(key, JsonConvert.SerializeObject(value), options);

        }
    }
}
