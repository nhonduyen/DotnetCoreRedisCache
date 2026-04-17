using System;
using System.Collections.Generic;

namespace DotnetCoreRedisCache.Models
{
    public class ShoppingCart
    {
        public Guid CartId { get; set; } = Guid.Empty;
        public Guid UserId { get; set; } = Guid.Empty;
        public string Currency { get; set; } = string.Empty;
        public List<CartItem> Items { get; set; } = new();
        public Summary Summary { get; set; } = new();
        public ShippingAddress ShippingAddress { get; set; } = new();
        public PaymentMethod PaymentMethod { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CartItem
    {
        public Guid ProductId { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }

    public class Summary
    {
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public decimal Discount { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class ShippingAddress
    {
        public string Name { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
    }

    public class PaymentMethod
    {
        public string Type { get; set; } = string.Empty;
        public string Last4 { get; set; } = string.Empty;
    }
}
