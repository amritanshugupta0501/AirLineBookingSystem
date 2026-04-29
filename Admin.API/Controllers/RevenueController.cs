using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RevenueController : ControllerBase
    {
        // Mock revenue controller for Phase 4 example. In a real system,
        // this would query a data warehouse or aggregate Booking data.
        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            var summary = new
            {
                TotalRevenue = 1500000.00M,
                TotalBookings = 1250,
                TopRoutes = new[]
                {
                    new { Route = "DEL-BOM", Revenue = 500000.00M },
                    new { Route = "BLR-DEL", Revenue = 350000.00M }
                }
            };

            return Ok(summary);
        }
    }
}
