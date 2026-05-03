using Microsoft.EntityFrameworkCore;
using Schedule.API.Models;

namespace Schedule.API.Data
{
    public class ScheduleDbContext : DbContext
    {
        public ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : base(options) { }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorSchedule> Schedules { get; set; }
    }
}
