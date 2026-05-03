using Common.Authentication;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedule.API.Commands;
using Schedule.API.Data;
using Schedule.API.Models;

namespace Schedule.API.Handlers
{
    public class CreateScheduleHandler : IRequestHandler<CreateScheduleCommand, Guid>
    {
        private readonly ScheduleDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public CreateScheduleHandler(ScheduleDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(
                d => d.UserId == _currentUser.UserId, cancellationToken);

            if (doctor == null)
            {
                doctor = new Doctor
                {
                    Id = Guid.NewGuid(),
                    UserId = _currentUser.UserId,
                    Name = _currentUser.Email, 
                    Specialty = "General"
                };
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var schedule = new DoctorSchedule
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                SlotDurationMinutes = request.SlotDurationMinutes,
                IsActive = true
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync(cancellationToken);

            return schedule.Id;
        }
    }
}
