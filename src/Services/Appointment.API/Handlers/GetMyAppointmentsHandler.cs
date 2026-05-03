using Appointment.API.Data;
using Appointment.API.Models;
using Appointment.API.Queries;
using Common.Authentication;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Appointment.API.Handlers
{
    public class GetMyAppointmentsHandler : IRequestHandler<GetMyAppointmentsQuery, List<AppointmentDto>>
    {
        private readonly AppointmentDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyAppointmentsHandler(AppointmentDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<AppointmentDto>> Handle(GetMyAppointmentsQuery request, CancellationToken cancellationToken)
        {
            var appointments = await _context.Appointments
                .Where(a => a.PatientId == _currentUser.UserId && a.Status != AppointmentStatus.Cancelled)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentDto(
                    a.Id,
                    a.DoctorId,
                    a.AppointmentDate,
                    a.StartTime,
                    a.EndTime,
                    a.Reason,
                    a.Status.ToString()
                ))
                .ToListAsync(cancellationToken);

            return appointments;
        }
    }
}
