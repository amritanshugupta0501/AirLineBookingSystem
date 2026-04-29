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
    public class ScheduleController : ControllerBase
    {
        private readonly StaffDbContext _context;

        public ScheduleController(StaffDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var schedules = await _context.FlightSchedules.ToListAsync();
            return Ok(schedules);
        }

        [HttpGet("{flightNumber}")]
        public async Task<IActionResult> GetByFlightNumber(string flightNumber)
        {
            var schedule = await _context.FlightSchedules
                .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);

            if (schedule == null) return NotFound();

            return Ok(schedule);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FlightSchedule schedule)
        {
            _context.FlightSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByFlightNumber), new { flightNumber = schedule.FlightNumber }, schedule);
        }

        [HttpPut("{flightNumber}/status")]
        public async Task<IActionResult> UpdateStatus(string flightNumber, [FromBody] UpdateStatusDto dto)
        {
            var schedule = await _context.FlightSchedules
                .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);

            if (schedule == null) return NotFound();

            schedule.Status = dto.Status;
            schedule.DelayReason = dto.DelayReason;

            await _context.SaveChangesAsync();

            return Ok(schedule);
        }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string? DelayReason { get; set; }
    }
}
