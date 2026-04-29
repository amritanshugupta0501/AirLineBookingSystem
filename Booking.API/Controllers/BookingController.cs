using Booking.API.Data;
using Booking.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly BookingDbContext _context;
        private readonly IDistributedCache _cache;

        public BookingController(BookingDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpPost("hold")]
        public async Task<IActionResult> HoldSeat([FromBody] HoldSeatRequest request)
        {
            var seat = await _context.Seats.FindAsync(request.SeatId);

            if (seat == null) return NotFound("Seat not found.");

            if (seat.Status != "Available")
                return BadRequest("Seat is no longer available.");

            // Check if another hold exists in Redis
            var cacheKey = $"seat-hold:{request.SeatId}";
            var existingHold = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(existingHold))
            {
                return Conflict("Seat is currently held by another user.");
            }

            seat.Status = "Held";
            seat.HeldByUserId = request.UserId;
            seat.HoldExpiry = DateTime.UtcNow.AddMinutes(15);

            // Set hold in Redis with 15 minute TTL
            await _cache.SetStringAsync(cacheKey, request.UserId, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Seat held successfully for 15 minutes.", Seat = seat });
            }
            catch (DbUpdateConcurrencyException)
            {
                // This guarantees nobody else grabbed it in the same millisecond
                return Conflict("Concurrency conflict: The seat was locked by another user just now.");
            }
        }

        [HttpPost("book")]
        public async Task<IActionResult> BookSeat([FromBody] BookSeatRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var seat = await _context.Seats.FindAsync(request.SeatId);
                
                // Verify hold in Redis
                var cacheKey = $"seat-hold:{request.SeatId}";
                var holdingUser = await _cache.GetStringAsync(cacheKey);

                if (seat == null || holdingUser != request.UserId)
                {
                    return BadRequest("Seat is not currently held by you or expired.");
                }

                // Remove the hold from Redis since we are booking it
                await _cache.RemoveAsync(cacheKey);

                // Generate 6-char PNR
                string pnr = GeneratePNR();

                var booking = new BookingRecord
                {
                    PNR = pnr,
                    SeatId = seat.Id,
                    UserId = request.UserId,
                    TotalAmount = request.Amount,
                    PaymentStatus = "PENDING", // Confirmed via Webhook later
                    BookingTime = DateTime.UtcNow
                };

                seat.Status = "Booked";

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = "Booking created. Awaiting payment...", PNR = pnr });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Transaction failed: " + ex.Message);
            }
        }

        private string GeneratePNR()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class HoldSeatRequest
    {
        public int SeatId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class BookSeatRequest
    {
        public int SeatId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
