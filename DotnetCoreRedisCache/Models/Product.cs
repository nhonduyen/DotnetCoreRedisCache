using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetCoreRedisCache.Models
{
    public class Product
    {
        public Product()
        {

        }

        public Product(string name, string description, string category, decimal price)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            Category = category;
            Price = price;
            Active = true;
        }

        public Guid Id { get; set; }

        [StringLength(80, MinimumLength = 4)]
        public string? Name { get; set; }

        [StringLength(80, MinimumLength = 4)]
        public string? Description { get; set; }

        [StringLength(80, MinimumLength = 4)]
        public string? Category { get; set; }

        public bool Active { get; set; } = true;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
    }
}
