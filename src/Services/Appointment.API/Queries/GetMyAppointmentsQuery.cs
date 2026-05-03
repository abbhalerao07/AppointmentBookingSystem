using Common.CQRS;

namespace Appointment.API.Queries
{
    public record GetMyAppointmentsQuery : IQuery<List<AppointmentDto>>;

    public record AppointmentDto(
        Guid Id,
        Guid DoctorId,
        DateTime AppointmentDate,
        TimeSpan StartTime,
        TimeSpan EndTime,
        string Reason,
        string Status
    );

    
}
