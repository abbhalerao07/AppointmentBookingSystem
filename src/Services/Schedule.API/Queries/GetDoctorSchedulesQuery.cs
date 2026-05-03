using Common.CQRS;

namespace Schedule.API.Queries
{
    public record GetDoctorSchedulesQuery(Guid DoctorId) : IQuery<List<ScheduleDto>>;

    public record ScheduleDto(
        Guid Id,
        DayOfWeek DayOfWeek,
        TimeSpan StartTime,
        TimeSpan EndTime,
        int SlotDurationMinutes
    );
}
