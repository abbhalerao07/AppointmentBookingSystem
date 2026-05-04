using Appointment.API.Commands;
using Appointment.API.Data;
using Appointment.API.Services;
using Common.Authentication;
using Common.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Appointment.API.Handlers
{
    public class BookAppointmentHandler : IRequestHandler<BookAppointmentCommand, Guid>
    {
        private readonly AppointmentDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IRabbitMqPublisher _publisher;
        private readonly IScheduleApiClient _scheduleApiClient;

        public BookAppointmentHandler(
            AppointmentDbContext context,
            ICurrentUserService currentUser,
            IRabbitMqPublisher publisher,
            IScheduleApiClient scheduleApiClient)
        {
            _context = context;
            _currentUser = currentUser;
            _publisher = publisher;
            _scheduleApiClient = scheduleApiClient;
        }

        public async Task<Guid> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
        {
            var schedules = await _scheduleApiClient.GetDoctorSchedulesAsync(request.DoctorId, cancellationToken);

            var matchingSchedules = schedules
                .Where(x => x.DayOfWeek == request.AppointmentDate.DayOfWeek)
                .OrderBy(x => x.StartTime)
                .ToList();

            if (!matchingSchedules.Any())
                throw new InvalidOperationException("Doctor will not be available on that day");

            var requestedDurationMinutes = (int)(request.EndTime - request.StartTime).TotalMinutes;

            var fitsSchedule = matchingSchedules.Any(x =>
                request.StartTime >= x.StartTime &&
                request.EndTime <= x.EndTime &&
                requestedDurationMinutes == x.SlotDurationMinutes);

            if (!fitsSchedule)
                throw new InvalidOperationException("Selected time must match the doctor's slot duration");

            var hasConflict = await _context.Appointments
                .AnyAsync(a =>
                    a.DoctorId == request.DoctorId &&
                    a.AppointmentDate.Date == request.AppointmentDate.Date &&
                    a.StartTime < request.EndTime &&
                    a.EndTime > request.StartTime &&
                    a.Status != Models.AppointmentStatus.Cancelled,
                    cancellationToken);

            if (hasConflict)
                throw new InvalidOperationException("Time slot is already booked");

            var appointment = new Models.Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = _currentUser.UserId,
                DoctorId = request.DoctorId,
                AppointmentDate = request.AppointmentDate.Date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Reason = request.Reason,
                Status = Models.AppointmentStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(cancellationToken);

            _publisher.PublishAppointmentBooked(new AppointmentBookedEvent
            {
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                PatientEmail = _currentUser.Email,
                PatientName = _currentUser.Email,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime
            });

            return appointment.Id;
        }
    }
}
