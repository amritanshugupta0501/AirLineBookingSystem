using Flight.Search.API.Data;
using Flight.Search.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using MassTransit;
using Shared.Messages;

namespace Flight.Search.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Added as per architecture (Auth.API validates JWT before this)
    public class FlightSearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly IPublishEndpoint _publishEndpoint;

        public FlightSearchController(ApplicationDbContext context, IDistributedCache cache, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _cache = cache;
            _publishEndpoint = publishEndpoint;
        }

        // UC-1: One-Way Search
        [HttpGet("oneway")]
        public async Task<ActionResult<IEnumerable<Models.Flight>>> SearchOneWay(
            [FromQuery] string origin, 
            [FromQuery] string destination, 
            [FromQuery] DateTime date)
        {
            // Publish Analytics Event
            await _publishEndpoint.Publish<FlightSearchedEvent>(new {
                Origin = origin,
                Destination = destination,
                SearchTime = DateTime.UtcNow
            });

            // Cache Key
            var cacheKey = $"oneway:{origin}:{destination}:{date:yyyyMMdd}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedFlights = JsonSerializer.Deserialize<List<Models.Flight>>(cachedData);
                return Ok(cachedFlights);
            }

            var flights = await _context.Flights
                .Where(f => f.Origin == origin && 
                            f.Destination == destination && 
                            f.DepartureDate.Date == date.Date)
                .ToListAsync();

            if (flights.Any())
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(flights), cacheOptions);
            }

            return Ok(flights);
        }

        // UC-2: Round-Trip Search
        [HttpGet("roundtrip")]
        public async Task<ActionResult<Dictionary<string, IList<Models.Flight>>>> SearchRoundTrip(
            [FromQuery] string origin, 
            [FromQuery] string destination, 
            [FromQuery] DateTime outboundDate, 
            [FromQuery] DateTime returnDate)
        {
            // Publish Analytics Event
            await _publishEndpoint.Publish<FlightSearchedEvent>(new {
                Origin = origin,
                Destination = destination,
                SearchTime = DateTime.UtcNow
            });

            // We could cache this just like one-way, omitting for brevity to show varied approaches
            var outboundFlights = await _context.Flights
                .Where(f => f.Origin == origin && 
                            f.Destination == destination && 
                            f.DepartureDate.Date == outboundDate.Date)
                .ToListAsync();

            var returnFlights = await _context.Flights
                .Where(f => f.Origin == destination && 
                            f.Destination == origin && 
                            f.DepartureDate.Date == returnDate.Date)
                .ToListAsync();

            var result = new Dictionary<string, IList<Models.Flight>>
            {
                { "outbound", outboundFlights },
                { "return", returnFlights }
            };

            return Ok(result);
        }

        // UC-3: Filter Results
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Models.Flight>>> FilterFlights(
            [FromQuery] string? origin,
            [FromQuery] string? destination,
            [FromQuery] DateTime? date,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? airline,
            [FromQuery] int? maxStops,
            [FromQuery] string? seatClass)
        {
            var query = _context.Flights.AsQueryable();

            if (!string.IsNullOrEmpty(origin))
                query = query.Where(f => f.Origin == origin);

            if (!string.IsNullOrEmpty(destination))
                query = query.Where(f => f.Destination == destination);

            if (date.HasValue)
                query = query.Where(f => f.DepartureDate.Date == date.Value.Date);

            if (maxPrice.HasValue)
                query = query.Where(f => f.Price <= maxPrice.Value);

            if (!string.IsNullOrEmpty(airline))
                query = query.Where(f => f.Airline == airline);

            if (maxStops.HasValue)
                query = query.Where(f => f.Stops <= maxStops.Value);

            if (!string.IsNullOrEmpty(seatClass))
                query = query.Where(f => f.SeatClass == seatClass);

            var flights = await query.ToListAsync();
            return Ok(flights);
        }
    }
}
