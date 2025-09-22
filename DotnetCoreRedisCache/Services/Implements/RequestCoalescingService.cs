using DotnetCoreRedisCache.Services.Interfaces;
using RedLockNet;
using System.Collections.Concurrent;

namespace DotnetCoreRedisCache.Services.Implements
{
    public class RequestCoalescingService : IRequestCoalescingService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDistributedLockFactory _lockFactory;
        private readonly ILogger<RequestCoalescingService> _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _localSemaphores;

        public RequestCoalescingService(
            IServiceProvider serviceProvider,
            IDistributedLockFactory lockFactory, 
            ILogger<RequestCoalescingService> logger)
        {
            _serviceProvider = serviceProvider;
            _lockFactory = lockFactory;
            _logger = logger;
            _localSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        public async Task<T> GetOrCreateAsync<T>(
            string key, 
            Func<Task<T>> factory, 
            TimeSpan? expiration = null,
            TimeSpan? lockTimeout = null, 
            TimeSpan? retryTime = null,
            TimeSpan? waitTime = null)
        {
            var cacheKey = $"cache:{key}";
            var lockKey = $"lock:{key}";
            var defaultExpiration = expiration ?? TimeSpan.FromMinutes(5);
            var defaultLockTimeout = lockTimeout ?? TimeSpan.FromSeconds(30);
            var defaultRetryTime = retryTime ?? TimeSpan.FromSeconds(5); // Total time to keep retrying
            var defaultWaitTime = waitTime ?? TimeSpan.FromMilliseconds(100); // Time between retries

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var _distributedCacheService = scope.ServiceProvider.GetRequiredService<IDistributedCacheService>();

            // First, try to get from cache
            var cachedValue = await _distributedCacheService.GetDataAsync<T>(cacheKey);
            if (cachedValue != null)
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return cachedValue;
            }

            // Use local semaphore to prevent multiple requests on same instance
            var localSemaphore = _localSemaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await localSemaphore.WaitAsync();

            try
            {
                await using var distributedLock = await _lockFactory.CreateLockAsync(
                    lockKey, 
                    defaultExpiration, 
                    defaultWaitTime,
                    defaultRetryTime);

                if (distributedLock.IsAcquired)
                {
                    _logger.LogDebug("Acquired distributed lock for key: {Key}", key);
                    // Double-check cache after acquiring local lock
                    cachedValue = await _distributedCacheService.GetDataAsync<T>(cacheKey);
                    if (cachedValue != null)
                    {
                        _logger.LogDebug("Cache hit for key: {Key}", key);
                        return cachedValue;
                    }

                    // Execute the expensive operation
                    _logger.LogInformation("Executing factory for key: {Key}", key);
                    var result = await factory();

                    // Store in cache
                    await _distributedCacheService.SetDataAsync(cacheKey, result, DateTimeOffset.UtcNow.Add(defaultExpiration));

                    return result;
                }
                else
                {
                    _logger.LogWarning($"Failed to acquire distributed lock for key: {key} after {retryTime}ms");
                    // Since Redlock already retried, we can be more confident this is contention
                    // Wait a bit longer and do final cache check
                    await Task.Delay(200);
                    cachedValue = await _distributedCacheService.GetDataAsync<T>(cacheKey);

                    if (cachedValue != null)
                    {
                        _logger.LogInformation("Found cached value after lock timeout for key: {Key}", key);
                        return cachedValue;
                    }

                    // Final option: execute without lock but log the contention
                    _logger.LogWarning("High contention detected - executing factory without lock for key: {Key}", key);
                    return await factory();
                }
            }
            finally
            {
                localSemaphore.Release();
            }
            
        }
    }
}
