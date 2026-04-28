using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Passenger.API.Data;
using System.Threading.Tasks;
using PassengerModel = Passenger.API.Models.Passenger;

namespace Passenger.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PassengerController : ControllerBase
    {
        private readonly PassengerDbContext _context;
        private readonly IValidator<PassengerModel> _validator;

        public PassengerController(PassengerDbContext context, IValidator<PassengerModel> validator)
        {
            _context = context;
            _validator = validator;
        }

        /// <summary>Register a new passenger with passport validation.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PassengerModel passenger)
        {
            var result = await _validator.ValidateAsync(passenger);
            if (!result.IsValid)
                return BadRequest(result.Errors);

            _context.Passengers.Add(passenger);

            try
            {
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = passenger.Id }, passenger);
            }
            catch (DbUpdateException)
            {
                return Conflict("A passenger with this email or passport number already exists.");
            }
        }

        /// <summary>Fetch a passenger by their database ID.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var passenger = await _context.Passengers.FindAsync(id);
            return passenger is null ? NotFound() : Ok(passenger);
        }

        /// <summary>Look up a passenger by email (used by Booking.API to attach passenger details).</summary>
        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var passenger = await _context.Passengers
                .FirstOrDefaultAsync(p => p.Email == email);
            return passenger is null ? NotFound() : Ok(passenger);
        }
    }
}
