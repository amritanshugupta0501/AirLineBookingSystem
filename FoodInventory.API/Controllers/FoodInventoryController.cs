using FoodInventory.API.Data;
using FoodInventory.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "AirlineStaff,Admin")]
    public class FoodInventoryController : ControllerBase
    {
        private readonly FoodInventoryDbContext _context;

        public FoodInventoryController(FoodInventoryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.FoodItems.ToListAsync());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStock(string id, [FromBody] UpdateStockDto dto)
        {
            var item = await _context.FoodItems.FindAsync(id);
            if (item == null) return NotFound();

            item.StockQuantity = dto.Quantity;
            await _context.SaveChangesAsync();
            return Ok(item);
        }
    }

    public class UpdateStockDto
    {
        public int Quantity { get; set; }
    }
}
