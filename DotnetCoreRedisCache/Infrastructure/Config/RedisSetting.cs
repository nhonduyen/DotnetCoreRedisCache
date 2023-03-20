namespace DotnetCoreRedisCache.Infrastructure.Config
{
    public class RedisSetting
    {
        public string RedisUrl { get; set; }
        public bool Ssl { get; set; }
        public double AbsoluteExpirationRelativeToNow { get; set; }
        public double SlidingExpiration { get; set; }
    }
}
