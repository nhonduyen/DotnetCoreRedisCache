using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace DotnetCoreRedisCache.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StringController : ControllerBase
    {
        private readonly IDatabaseAsync _cache;
        private readonly ILogger<StringController> _logger;
        private const string ViewKey = "view";

        public StringController(IConnectionMultiplexer connectionMultiplexer, ILogger<StringController> logger)
        {
            _cache = connectionMultiplexer.GetDatabase();
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<long>> Increase()
        {
            var value = await _cache.StringIncrementAsync(ViewKey);
            _logger.LogInformation("Increased Redis key '{ViewKey}' to {Value}", ViewKey, value);
            return Ok(value);
        }

        [HttpPost]
        public async Task<ActionResult<long>> Decrease()
        {
            var value = await _cache.StringDecrementAsync(ViewKey);
            _logger.LogInformation("Decreased Redis key '{ViewKey}' to {Value}", ViewKey, value);
            return Ok(value);
        }
    }
}
