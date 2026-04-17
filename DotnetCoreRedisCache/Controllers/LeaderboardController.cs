using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace DotnetCoreRedisCache.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        private readonly IDatabaseAsync _cache;
        private readonly ILogger<LeaderboardController> _logger;
        private const string LeaderboardKey = "leaderboard:scores";

        public LeaderboardController(IConnectionMultiplexer connectionMultiplexer, ILogger<LeaderboardController> logger)
        {
            _cache = connectionMultiplexer.GetDatabase();
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<LeaderboardResponse>> InsertScore([FromBody] InsertScoreRequest request)
        {
            if (request.UserId == Guid.Empty)
            {
                return BadRequest("UserId is required.");
            }

            var added = await _cache.SortedSetAddAsync(LeaderboardKey, request.UserId.ToString(), request.Score);
            _logger.LogInformation("Inserted score {Score} for user {UserId}. Added new member: {Added}.", request.Score, request.UserId, added);

            var topScores = await _cache.SortedSetRangeByRankWithScoresAsync(
                LeaderboardKey,
                -10,
                -1,
                Order.Descending);

            var topPlayers = new List<LeaderboardEntry>(topScores.Length);
            foreach (var entry in topScores)
            {
                if (Guid.TryParse(entry.Element, out var userId))
                {
                    topPlayers.Add(new LeaderboardEntry
                    {
                        UserId = userId,
                        Score = entry.Score
                    });
                }
            }

            return Ok(new LeaderboardResponse
            {
                TopPlayers = topPlayers
            });
        }

        [HttpGet]
        public async Task<ActionResult<LeaderboardResponse>> GetTop10()
        {
            var topScores = await _cache.SortedSetRangeByRankWithScoresAsync(
                LeaderboardKey,
                -10,
                -1,
                Order.Descending);

            var topPlayers = new List<LeaderboardEntry>(topScores.Length);
            foreach (var entry in topScores)
            {
                if (Guid.TryParse(entry.Element, out var userId))
                {
                    topPlayers.Add(new LeaderboardEntry
                    {
                        UserId = userId,
                        Score = entry.Score
                    });
                }
            }

            return Ok(new LeaderboardResponse
            {
                TopPlayers = topPlayers
            });
        }
    }

    public class InsertScoreRequest
    {
        public Guid UserId { get; set; } = Guid.Empty;
        public double Score { get; set; }
    }

    public class LeaderboardEntry
    {
        public Guid UserId { get; set; }
        public double Score { get; set; }
    }

    public class LeaderboardResponse
    {
        public List<LeaderboardEntry> TopPlayers { get; set; } = new();
    }
}
