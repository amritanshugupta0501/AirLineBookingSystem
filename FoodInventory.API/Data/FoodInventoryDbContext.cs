using FoodInventory.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodInventory.API.Data
{
    public class FoodInventoryDbContext : DbContext
    {
        public FoodInventoryDbContext(DbContextOptions<FoodInventoryDbContext> options) : base(options) { }

        public DbSet<FoodItem> FoodItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Seed some default data
            modelBuilder.Entity<FoodItem>().HasData(
                new FoodItem { Id = "F001", Name = "Vegetarian Sandwich", Category = "Meal", StockQuantity = 50, Price = 250 },
                new FoodItem { Id = "F002", Name = "Chicken Wrap", Category = "Meal", StockQuantity = 40, Price = 300 },
                new FoodItem { Id = "B001", Name = "Coffee", Category = "Beverage", StockQuantity = 100, Price = 100 },
                new FoodItem { Id = "B002", Name = "Water Bottle", Category = "Beverage", StockQuantity = 200, Price = 50 }
            );
        }
    }
}
