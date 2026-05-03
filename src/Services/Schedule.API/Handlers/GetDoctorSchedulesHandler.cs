using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedule.API.Data;
using Schedule.API.Queries;

namespace Schedule.API.Handlers
{
    public class GetDoctorSchedulesHandler : IRequestHandler<GetDoctorSchedulesQuery, List<ScheduleDto>>
    {
        private readonly ScheduleDbContext _context;

        public GetDoctorSchedulesHandler(ScheduleDbContext context)
        {
            _context = context;
        }

        public async Task<List<ScheduleDto>> Handle(GetDoctorSchedulesQuery request, CancellationToken cancellationToken)
        {
            var schedules = await _context.Schedules
                .Where(s => s.DoctorId == request.DoctorId && s.IsActive)
                .Select(s => new ScheduleDto(s.Id, s.DayOfWeek, s.StartTime, s.EndTime, s.SlotDurationMinutes))
                .ToListAsync(cancellationToken);

            return schedules;
        }
    }
}
