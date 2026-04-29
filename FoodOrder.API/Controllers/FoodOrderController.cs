using FoodOrder.API.Data;
using FoodOrder.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Any authenticated user (passenger) can order
    public class FoodOrderController : ControllerBase
    {
        private readonly FoodOrderDbContext _context;

        public FoodOrderController(FoodOrderDbContext context)
        {
            _context = context;
        }

        [HttpGet("{pnr}")]
        public async Task<IActionResult> GetByPnr(string pnr)
        {
            var orders = await _context.Orders
                .Where(o => o.PNR == pnr)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByPnr), new { pnr = order.PNR }, order);
        }
    }
}
