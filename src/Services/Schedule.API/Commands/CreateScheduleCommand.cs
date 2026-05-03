using Common.CQRS;

namespace Schedule.API.Commands
{
    public record CreateScheduleCommand(
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int SlotDurationMinutes
) : ICommand<Guid>;
}
