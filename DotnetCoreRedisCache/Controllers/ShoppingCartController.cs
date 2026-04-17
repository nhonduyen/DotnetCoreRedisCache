using DotnetCoreRedisCache.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Linq;

namespace DotnetCoreRedisCache.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IDatabaseAsync _cache;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(IConnectionMultiplexer connectionMultiplexer, ILogger<ShoppingCartController> logger)
        {
            _cache = connectionMultiplexer.GetDatabase();
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ShoppingCart>> UpdateItemQuantity([FromBody] UpdateCartItemQuantityRequest request)
        {
            if (request.CartId == Guid.Empty || request.ProductId == Guid.Empty)
            {
                return BadRequest("CartId and ProductId are required.");
            }

            var cartKey = $"cart:{request.CartId}";
            if (!await _cache.KeyExistsAsync(cartKey))
            {
                var newItem = new CartItem
                {
                    ProductId = request.ProductId,
                    Name = "Unknown Item",
                    Price = 0m,
                    Quantity = request.Quantity,
                    Total = 0m
                };

                var newSummary = new Summary
                {
                    Subtotal = newItem.Total,
                    Tax = 0m,
                    Shipping = 0m,
                    Discount = 0m,
                    GrandTotal = newItem.Total
                };

                await _cache.HashSetAsync(cartKey, request.ProductId.ToString(), JsonConvert.SerializeObject(newItem));
                await _cache.HashSetAsync(cartKey, "summary", JsonConvert.SerializeObject(newSummary));

                _logger.LogInformation("Created dummy cart {CartId} with item {ProductId}.", request.CartId, request.ProductId);

                var createdCart = new ShoppingCart
                {
                    CartId = request.CartId,
                    Items = new List<CartItem> { newItem },
                    Summary = newSummary
                };

                return Ok(createdCart);
            }

            var itemValue = await _cache.HashGetAsync(cartKey, request.ProductId.ToString());
            if (!itemValue.HasValue)
            {
                var newItem = new CartItem
                {
                    ProductId = request.ProductId,
                    Name = "Unknown Item",
                    Price = 0m,
                    Quantity = request.Quantity,
                    Total = 0m
                };

                await _cache.HashSetAsync(cartKey, request.ProductId.ToString(), JsonConvert.SerializeObject(newItem));
                itemValue = await _cache.HashGetAsync(cartKey, request.ProductId.ToString());
            }

            var item = JsonConvert.DeserializeObject<CartItem>(itemValue!);
            if (item == null)
            {
                return BadRequest("Cart item data is invalid.");
            }

            item.Quantity = request.Quantity;
            item.Total = item.Price * request.Quantity;
            await _cache.HashSetAsync(cartKey, request.ProductId.ToString(), JsonConvert.SerializeObject(item));

            var allFields = await _cache.HashGetAllAsync(cartKey);
            var items = allFields
                .Where(field => field.Name != "summary")
                .Select(field => JsonConvert.DeserializeObject<CartItem>(field.Value))
                .Where(cartItem => cartItem != null)
                .Cast<CartItem>()
                .ToList();

            Summary? summary = null;
            var summaryField = allFields.FirstOrDefault(field => field.Name == "summary");
            if (summaryField.Value.HasValue)
            {
                summary = JsonConvert.DeserializeObject<Summary>(summaryField.Value!);
            }

            if (summary != null)
            {
                summary.Subtotal = items.Sum(x => x.Total);
                summary.GrandTotal = summary.Subtotal + summary.Tax + summary.Shipping - summary.Discount;
                await _cache.HashSetAsync(cartKey, "summary", JsonConvert.SerializeObject(summary));
            }

            _logger.LogInformation("Updated quantity for cart {CartId}, item {ProductId} to {Quantity}.", request.CartId, request.ProductId, request.Quantity);

            var cart = new ShoppingCart
            {
                CartId = request.CartId,
                Items = items,
                Summary = summary ?? new Summary(),
            };

            return Ok(cart);
        }
    }

    public class UpdateCartItemQuantityRequest
    {
        public Guid CartId { get; set; } = Guid.Empty;
        public Guid ProductId { get; set; } = Guid.Empty;
        public int Quantity { get; set; }
    }
}
