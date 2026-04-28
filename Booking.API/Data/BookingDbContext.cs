using Booking.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking.API.Data
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

        public DbSet<Seat> Seats { get; set; }
        public DbSet<BookingRecord> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Map the Timestamp rowversion safely for SQL Server
            modelBuilder.Entity<Seat>()
                .Property(s => s.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        }
    }
}
