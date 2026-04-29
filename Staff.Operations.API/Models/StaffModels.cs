using System;
using System.ComponentModel.DataAnnotations;

namespace Staff.Operations.API.Models
{
    // Tracks scheduled flights and their live status
    public class FlightSchedule
    {
        [Key] public int Id { get; set; }
        [Required] public string FlightNumber { get; set; } = string.Empty;
        [Required] public string Origin { get; set; } = string.Empty;
        [Required] public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        // Scheduled | Delayed | Boarding | Departed | Cancelled
        public string Status { get; set; } = "Scheduled";
        public string? DelayReason { get; set; }
    }

    // Per-flight passenger manifest entry
    public class PassengerManifest
    {
        [Key] public int Id { get; set; }
        [Required] public string FlightNumber { get; set; } = string.Empty;
        [Required] public string PassengerName { get; set; } = string.Empty;
        [Required] public string PNR { get; set; } = string.Empty;
        [Required] public string SeatNumber { get; set; } = string.Empty;
        public bool CheckedIn { get; set; } = false;
        public DateTime? CheckInTime { get; set; }
    }
}
