using System;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace DotnetCoreRedisCache.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SetController : ControllerBase
    {
        private readonly IDatabaseAsync _cache;
        private readonly ILogger<SetController> _logger;
        private const string LikesKeyPattern = "post:{0}:likes";

        public SetController(IConnectionMultiplexer connectionMultiplexer, ILogger<SetController> logger)
        {
            _cache = connectionMultiplexer.GetDatabase();
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<LikeResponse>> LikePost([FromBody] LikePostRequest request)
        {
            if (request.UserId == Guid.Empty || request.PostId == Guid.Empty)
            {
                return BadRequest("UserId and PostId are required.");
            }

            var likesKey = string.Format(LikesKeyPattern, request.PostId);
            var added = await _cache.SetAddAsync(likesKey, request.UserId.ToString());
            var totalLikes = await _cache.SetLengthAsync(likesKey);

            _logger.LogInformation("User {UserId} liked post {PostId}: added={Added}.", request.UserId, request.PostId, added);

            return Ok(new LikeResponse
            {
                PostId = request.PostId,
                UserId = request.UserId,
                Liked = added,
                TotalLikes = totalLikes
            });
        }
    }

    public class LikePostRequest
    {
        public Guid UserId { get; set; } = Guid.Empty;
        public Guid PostId { get; set; } = Guid.Empty;
    }

    public class LikeResponse
    {
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public bool Liked { get; set; }
        public long TotalLikes { get; set; }
    }
}
