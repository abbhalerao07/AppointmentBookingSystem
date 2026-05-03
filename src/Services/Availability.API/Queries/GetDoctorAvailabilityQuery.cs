using Common.CQRS;

namespace Availability.API.Queries
{
    public record GetDoctorAvailabilityQuery(Guid DoctorId, DateTime Date)
    : IQuery<DoctorAvailabilityResponse>;

    public record DoctorAvailabilityResponse(
        Guid DoctorId,
        DateTime Date,
        List<AvailabilitySlotDto> Slots
    );

    public record AvailabilitySlotDto(
        TimeSpan StartTime,
        TimeSpan EndTime,
        bool IsAvailable
    );

    public record ScheduleDto(
        Guid Id,
        DayOfWeek DayOfWeek,
        TimeSpan StartTime,
        TimeSpan EndTime,
        int SlotDurationMinutes
    );
}
