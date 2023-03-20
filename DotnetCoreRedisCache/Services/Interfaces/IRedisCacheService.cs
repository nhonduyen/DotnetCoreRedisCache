namespace DotnetCoreRedisCache.Services.Implements
{
    public interface IRedisCacheService
    {
        Task<T> GetData<T>(string key);
        Task<bool> SetData<T>(string key, T value, DateTimeOffset expirationTime);
        Task<bool> SetData<T>(string key, T value);
        Task<bool> DeleteData<T>(string key);
    }
}
