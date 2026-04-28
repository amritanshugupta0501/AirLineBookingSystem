using Flight.Search.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Flight.Search.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Models.Flight> Flights { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite Index for One-Way Search (UC-1)
            modelBuilder.Entity<Models.Flight>()
                .HasIndex(f => new { f.Origin, f.Destination, f.DepartureDate })
                .HasDatabaseName("IX_Flight_Origin_Destination_Date");
        }
    }
}
