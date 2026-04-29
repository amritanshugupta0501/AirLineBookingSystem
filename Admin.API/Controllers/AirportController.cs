using Admin.API.Data;
using Admin.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Admin.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AirportController : ControllerBase
    {
        private readonly AdminDbContext _context;

        public AirportController(AdminDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Airports.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Airport airport)
        {
            _context.Airports.Add(airport);
            try
            {
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetAll), new { id = airport.Id }, airport);
            }
            catch (DbUpdateException)
            {
                return Conflict("An airport with this code already exists.");
            }
        }
    }
}
