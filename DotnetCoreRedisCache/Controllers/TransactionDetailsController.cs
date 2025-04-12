using DotnetCoreRedisCache.Infrastructure.Data;
using DotnetCoreRedisCache.Infrastructure.Utility;
using DotnetCoreRedisCache.Models;
using DotnetCoreRedisCache.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DotnetCoreRedisCache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionDetailsController : ControllerBase
    {
        private readonly ProductDBContext _context;
        private readonly IDistributedCacheService _distributedCache;
        private readonly ILogger<TransactionDetailsController> _logger;

        public TransactionDetailsController(ProductDBContext context, IDistributedCacheService distributedCache, ILogger<TransactionDetailsController> logger)
        {
            _context = context;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<TransactionDetails>> PostTransactionDetails(TransactionDetails transactionDetails)
        {
            // Create a hash key using transaction id, dr and cr amount
            string idempotencyKey = HashGenerator.GetHash(transactionDetails);


            // check hash key is exists in the redis cache
            var cacheTransactionDetails = await _distributedCache.GetDataAsync<TransactionDetails>(idempotencyKey);

            if (cacheTransactionDetails is not null)
            {
                // if same value is already exists in the cache then return existing value. 
                _logger.LogInformation($"Cache {idempotencyKey} Transaction Details found. Return from cache");
                return cacheTransactionDetails;
            }

            // if input object is null return with a problem
            if (_context.TransactionDetails == null)
            {
                return Problem("Entity set 'AccountingContext.TransactionDetails'  is null.");
            }


            // Save into database
            _context.TransactionDetails.Add(transactionDetails);
            var insertedRecord = await _context.SaveChangesAsync();

            _logger.LogInformation($"{insertedRecord} Transaction Details inserted");

            // Set value into cache after save
            // value will be removed after 10s
            // It will be removed after 5s, if it is not requested within 5s
            await _distributedCache.SetDataAsync<TransactionDetails>(idempotencyKey, transactionDetails);

            _logger.LogInformation($"{insertedRecord} Transaction Details cached");


            return CreatedAtAction("GetTransactionDetails", new { id = transactionDetails.Id }, transactionDetails);
        }
    }
}
