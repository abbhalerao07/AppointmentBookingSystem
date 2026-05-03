using Common.CQRS;

namespace Appointment.API.Commands
{
    public record CancelAppointmentCommand(Guid AppointmentId, string Reason) : ICommand;
}
