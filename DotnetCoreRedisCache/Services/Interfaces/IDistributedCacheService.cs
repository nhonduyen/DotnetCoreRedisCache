namespace DotnetCoreRedisCache.Services.Interfaces
{
    public interface IDistributedCacheService
    {
        Task<T> GetDataAsync<T>(string key);
        Task SetDataAsync<T>(string key, T value, DateTimeOffset expirationTime);
        Task SetDataAsync<T>(string key, T value);
        Task DeleteDataAsync<T>(string key);
    }
}
