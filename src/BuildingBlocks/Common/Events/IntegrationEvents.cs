using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Events
{
    // Base integration event
    public abstract record IntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }

    // Appointment Booked Event - published to RabbitMQ
    public record AppointmentBookedEvent : IntegrationEvent
    {
        public Guid AppointmentId { get; init; }
        public Guid PatientId { get; init; }
        public Guid DoctorId { get; init; }
        public string PatientEmail { get; init; } = string.Empty;
        public string PatientName { get; init; } = string.Empty;
        public DateTime AppointmentDate { get; init; }
        public TimeSpan StartTime { get; init; }
        public TimeSpan EndTime { get; init; }
    }

    // Appointment Cancelled Event
    public record AppointmentCancelledEvent : IntegrationEvent
    {
        public Guid AppointmentId { get; init; }
        public string PatientEmail { get; init; } = string.Empty;
        public DateTime AppointmentDate { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}
