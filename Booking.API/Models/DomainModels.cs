using System;
using System.ComponentModel.DataAnnotations;

namespace Booking.API.Models
{
    public class Seat
    {
        [Key]
        public int Id { get; set; }
        
        public string FlightNumber { get; set; } = string.Empty;
        
        public string SeatIdentifier { get; set; } = string.Empty;
        
        // Status: Available, Held, Booked
        public string Status { get; set; } = "Available"; 
        
        public DateTime? HoldExpiry { get; set; }
        
        public string HeldByUserId { get; set; } = string.Empty;

        [Timestamp] // Optimistic Concurrency Token
        public byte[] RowVersion { get; set; }
    }

    public class BookingRecord
    {
        [Key]
        public int Id { get; set; }
        
        public string PNR { get; set; } = string.Empty;
        
        public int SeatId { get; set; }
        
        public Seat Seat { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        
        public decimal TotalAmount { get; set; }
        
        // PENDING, CONFIRMED
        public string PaymentStatus { get; set; } = "PENDING";
        
        public DateTime BookingTime { get; set; }
    }
}
