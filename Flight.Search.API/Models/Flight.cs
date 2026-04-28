using System;
using System.ComponentModel.DataAnnotations;

namespace Flight.Search.API.Models
{
    public class Flight
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Origin { get; set; } = string.Empty;
        
        [Required]
        public string Destination { get; set; } = string.Empty;
        
        public DateTime DepartureDate { get; set; }
        
        public DateTime ArrivalDate { get; set; }
        
        public string Airline { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        
        public int Stops { get; set; }
        
        public string SeatClass { get; set; } = string.Empty;
    }
}
