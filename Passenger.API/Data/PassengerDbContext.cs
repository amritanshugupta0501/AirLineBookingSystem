using Microsoft.EntityFrameworkCore;
using PassengerModel = Passenger.API.Models.Passenger;

namespace Passenger.API.Data
{
    public class PassengerDbContext : DbContext
    {
        public PassengerDbContext(DbContextOptions<PassengerDbContext> options) : base(options) { }

        public DbSet<PassengerModel> Passengers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PassengerModel>(entity =>
            {
                entity.HasIndex(p => p.Email).IsUnique();
                entity.HasIndex(p => p.PassportNumber).IsUnique();
            });
        }
    }
}
