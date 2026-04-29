using System;
using System.ComponentModel.DataAnnotations;

namespace FoodOrder.API.Models
{
    public class Order
    {
        [Key] public int Id { get; set; }
        [Required] public string PNR { get; set; } = string.Empty;
        [Required] public string SeatNumber { get; set; } = string.Empty;
        [Required] public string MenuItemId { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        // Placed | Preparing | Delivered
        public string Status { get; set; } = "Placed";
        public DateTime OrderTime { get; set; } = DateTime.UtcNow;
    }
}
