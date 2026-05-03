using Availability.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Availability.API.Data
{
    public class DoctorAvailabilityDbContext : DbContext
    {
        public DoctorAvailabilityDbContext(DbContextOptions<DoctorAvailabilityDbContext> options)
            : base(options)
        {
        }

        public DbSet<BookedSlot> BookedSlots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookedSlot>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.AppointmentId).IsUnique();
            });
        }
    }
}
