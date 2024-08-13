using InnoShop.Services.ProductAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InnoShop.Services.ProductAPI.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //builder.Entity<Product>().HasData(new Product
            //{
            //    ProductId = 1,
            //    Name = "Apple pie",
            //    Description = "Extremely delicious pie",
            //    Price = 25,
            //    IsAvailable = true,
            //    UserId = "616e56f5-3406-4178-8691-b4e6325c8a37",
            //    Date = new DateTime(2024, 8, 12),
            //});
        }
    }
}
