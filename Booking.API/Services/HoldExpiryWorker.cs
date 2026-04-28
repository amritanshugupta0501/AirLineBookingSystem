using Booking.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Booking.API.Services
{
    public class HoldExpiryWorker : BackgroundService
    {
        private readonly ILogger<HoldExpiryWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public HoldExpiryWorker(ILogger<HoldExpiryWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Hold Expiry Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ReleaseExpiredHoldsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred releasing expired holds.");
                }

                // Run sweep every 1 minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ReleaseExpiredHoldsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

            var now = DateTime.UtcNow;

            var expiredHolds = await dbContext.Seats
                .Where(s => s.Status == "Held" && s.HoldExpiry != null && s.HoldExpiry < now)
                .ToListAsync();

            if (expiredHolds.Any())
            {
                _logger.LogInformation("Found {Count} expired seat holds. Releasing...", expiredHolds.Count);

                foreach (var seat in expiredHolds)
                {
                    seat.Status = "Available";
                    seat.HoldExpiry = null;
                    seat.HeldByUserId = string.Empty;
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
