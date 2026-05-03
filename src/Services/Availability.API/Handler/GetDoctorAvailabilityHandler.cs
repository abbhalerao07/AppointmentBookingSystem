using Availability.API.Data;
using Availability.API.Queries;
using Availability.API.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Availability.API.Handler
{
    public class GetDoctorAvailabilityHandler : IRequestHandler<GetDoctorAvailabilityQuery, DoctorAvailabilityResponse>
    {
        private readonly DoctorAvailabilityDbContext _context;
        private readonly IScheduleApiClient _scheduleApiClient;

        public GetDoctorAvailabilityHandler(
            DoctorAvailabilityDbContext context,
            IScheduleApiClient scheduleApiClient)
        {
            _context = context;
            _scheduleApiClient = scheduleApiClient;
        }

        public async Task<DoctorAvailabilityResponse> Handle(
            GetDoctorAvailabilityQuery request,
            CancellationToken cancellationToken)
        {
            var schedules = await _scheduleApiClient.GetDoctorSchedulesAsync(request.DoctorId, cancellationToken);

            var matchingSchedules = schedules
                .Where(x => x.DayOfWeek == request.Date.DayOfWeek)
                .OrderBy(x => x.StartTime)
                .ToList();

            var bookedSlots = await _context.BookedSlots
                .Where(x =>
                    x.DoctorId == request.DoctorId &&
                    x.AppointmentDate.Date == request.Date.Date &&
                    !x.IsCancelled)
                .ToListAsync(cancellationToken);

            var slots = new List<AvailabilitySlotDto>();

            foreach (var schedule in matchingSchedules)
            {
                if (schedule.SlotDurationMinutes <= 0)
                    continue;

                var slotDuration = TimeSpan.FromMinutes(schedule.SlotDurationMinutes);
                var current = schedule.StartTime;

                while (current + slotDuration <= schedule.EndTime)
                {
                    var slotEnd = current + slotDuration;

                    var isBooked = bookedSlots.Any(x =>
                        x.StartTime < slotEnd &&
                        x.EndTime > current);

                    slots.Add(new AvailabilitySlotDto(
                        StartTime: current,
                        EndTime: slotEnd,
                        IsAvailable: !isBooked));

                    current += slotDuration;
                }
            }

            return new DoctorAvailabilityResponse(
                DoctorId: request.DoctorId,
                Date: request.Date.Date,
                Slots: slots.OrderBy(x => x.StartTime).ToList());
        }
    }
}
