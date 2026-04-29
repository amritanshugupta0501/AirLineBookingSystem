using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staff.Operations.API.Data;
using Staff.Operations.API.Models;

namespace Staff.Operations.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "AirlineStaff,Admin")]
    public class ManifestController : ControllerBase
    {
        private readonly StaffDbContext _context;

        public ManifestController(StaffDbContext context)
        {
            _context = context;
        }

        [HttpGet("{flightNumber}")]
        public async Task<IActionResult> GetManifest(string flightNumber)
        {
            var manifest = await _context.PassengerManifests
                .Where(m => m.FlightNumber == flightNumber)
                .ToListAsync();

            return Ok(manifest);
        }

        [HttpPost]
        public async Task<IActionResult> AddToManifest([FromBody] PassengerManifest entry)
        {
            _context.PassengerManifests.Add(entry);
            try
            {
                await _context.SaveChangesAsync();
                return Ok(entry);
            }
            catch (DbUpdateException)
            {
                return Conflict("Passenger already on manifest for this flight.");
            }
        }

        [HttpPut("{id}/checkin")]
        public async Task<IActionResult> CheckIn(int id)
        {
            var entry = await _context.PassengerManifests.FindAsync(id);
            if (entry == null) return NotFound();

            entry.CheckedIn = true;
            entry.CheckInTime = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return Ok(entry);
        }
    }
}
