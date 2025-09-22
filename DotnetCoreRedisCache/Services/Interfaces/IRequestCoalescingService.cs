namespace DotnetCoreRedisCache.Services.Interfaces
{
    public interface IRequestCoalescingService
    {
        Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        TimeSpan? lockTimeout = null,
        TimeSpan ? retryTime = null,
        TimeSpan? waitTime = null);
    }
}
