using Microsoft.EntityFrameworkCore;
using Staff.Operations.API.Models;

namespace Staff.Operations.API.Data
{
    public class StaffDbContext : DbContext
    {
        public StaffDbContext(DbContextOptions<StaffDbContext> options) : base(options) { }

        public DbSet<FlightSchedule> FlightSchedules { get; set; }
        public DbSet<PassengerManifest> PassengerManifests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<FlightSchedule>()
                .HasIndex(f => f.FlightNumber);
            modelBuilder.Entity<PassengerManifest>()
                .HasIndex(p => new { p.FlightNumber, p.PNR }).IsUnique();
        }
    }
}
