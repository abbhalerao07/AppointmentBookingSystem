using Microsoft.EntityFrameworkCore;

namespace Appointment.API.Data
{
    public class AppointmentDbContext : DbContext
    {
        public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options) : base(options) { }

        public DbSet<Models.Appointment> Appointments { get; set; }
    }
}
