namespace DotnetCoreRedisCache.Services.Implements
{
    public interface IRedisCacheService
    {
        Task<T> GetDataAsync<T>(string key);
        Task<bool> SetDataAsync<T>(string key, T value, DateTimeOffset expirationTime);
        Task<bool> SetDataAsync<T>(string key, T value);
        Task<bool> DeleteDataAsync<T>(string key);
    }
}
