using Admin.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Admin.API.Data
{
    public class AdminDbContext : DbContext
    {
        public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }

        public DbSet<Airport> Airports { get; set; }
        public DbSet<Airline> Airlines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Airport>().HasIndex(a => a.Code).IsUnique();
            modelBuilder.Entity<Airline>().HasIndex(a => a.Code).IsUnique();
        }
    }
}
