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
    public class AirlineController : ControllerBase
    {
        private readonly AdminDbContext _context;

        public AirlineController(AdminDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Airlines.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Airline airline)
        {
            _context.Airlines.Add(airline);
            try
            {
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetAll), new { id = airline.Id }, airline);
            }
            catch (DbUpdateException)
            {
                return Conflict("An airline with this code already exists.");
            }
        }
    }
}
