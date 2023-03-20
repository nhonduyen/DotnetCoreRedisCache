using DotnetCoreRedisCache.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetCoreRedisCache.Infrastructure.Data
{
    public class ProductDBContext : DbContext
    {

        public ProductDBContext(DbContextOptions<ProductDBContext> options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
