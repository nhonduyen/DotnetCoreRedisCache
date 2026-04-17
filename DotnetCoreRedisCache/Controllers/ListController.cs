using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace DotnetCoreRedisCache.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ListController : ControllerBase
    {
        private readonly IDatabaseAsync _cache;
        private readonly ILogger<ListController> _logger;
        private const string UserQueueKey = "users:queue";
        private const string ActivityLogKeyPattern = "users:{0}:activity:log";
        private const int ActivityLogMaxLength = 50;

        public ListController(IConnectionMultiplexer connectionMultiplexer, ILogger<ListController> logger)
        {
            _cache = connectionMultiplexer.GetDatabase();
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<long>> EnqueueUser([FromBody] EnqueueUserRequest request)
        {
            if (request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest("UserId and Username are required.");
            }

            var queueItem = new UserQueueItem
            {
                UserId = request.UserId,
                Username = request.Username
            };

            var serialized = JsonConvert.SerializeObject(queueItem);
            var length = await _cache.ListRightPushAsync(UserQueueKey, serialized);

            _logger.LogInformation("Enqueued user {UserId} ({Username}). Queue length is {Length}.", request.UserId, request.Username, length);
            return Ok(length);
        }

        [HttpPost]
        public async Task<ActionResult<UserQueueItem?>> DequeueUser()
        {
            var itemValue = await _cache.ListLeftPopAsync(UserQueueKey);
            if (!itemValue.HasValue)
            {
                return NotFound("No users available in the queue.");
            }

            var item = JsonConvert.DeserializeObject<UserQueueItem>(itemValue!);
            if (item == null)
            {
                return BadRequest("Dequeued user data is invalid.");
            }

            _logger.LogInformation("Dequeued user {UserId} ({Username}) from queue.", item.UserId, item.Username);
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<long>> LogUserActivity([FromBody] LogUserActivityRequest request)
        {
            if (request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest("UserId and Username are required.");
            }

            var timestamp = request.Timestamp == default ? DateTime.UtcNow : request.Timestamp;
            var logMessage = $"user {request.Username} login at {timestamp:O}";
            var activity = new UserActivityLogItem
            {
                UserId = request.UserId,
                Log = logMessage
            };

            var activityLogKey = string.Format(ActivityLogKeyPattern, request.UserId);
            var serialized = JsonConvert.SerializeObject(activity);
            var length = await _cache.ListRightPushAsync(activityLogKey, serialized);
            await _cache.ListTrimAsync(activityLogKey, -ActivityLogMaxLength, -1);

            _logger.LogInformation("Logged activity for user {UserId}: {Log}", request.UserId, logMessage);
            return Ok(length);
        }
    }

    public class EnqueueUserRequest
    {
        public Guid UserId { get; set; } = Guid.Empty;
        public string Username { get; set; } = string.Empty;
    }

    public class LogUserActivityRequest
    {
        public Guid UserId { get; set; } = Guid.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class UserQueueItem
    {
        public Guid UserId { get; set; } = Guid.Empty;
        public string Username { get; set; } = string.Empty;
    }

    public class UserActivityLogItem
    {
        public Guid UserId { get; set; } = Guid.Empty;
        public string Log { get; set; } = string.Empty;
    }
}
