using System.ComponentModel.DataAnnotations;

namespace FoodInventory.API.Models
{
    public class FoodItem
    {
        [Key] public string Id { get; set; } = string.Empty;
        [Required] public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int StockQuantity { get; set; } = 0;
        public decimal Price { get; set; }
    }
}
