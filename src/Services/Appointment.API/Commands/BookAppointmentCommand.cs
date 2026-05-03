using Common.CQRS;

namespace Appointment.API.Commands
{
    public record BookAppointmentCommand(
    Guid DoctorId,
    DateTime AppointmentDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Reason
) : ICommand<Guid>;
}
