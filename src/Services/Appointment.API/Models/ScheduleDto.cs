namespace Appointment.API.Models
{
    public record ScheduleDto(
        Guid Id,
        DayOfWeek DayOfWeek,
        TimeSpan StartTime,
        TimeSpan EndTime,
        int SlotDurationMinutes
    );
}
