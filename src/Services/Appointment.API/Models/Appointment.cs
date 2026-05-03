namespace Appointment.API.Models
{
    public class Appointment
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum AppointmentStatus
    {
        Confirmed = 1,
        Cancelled = 2,
        Completed = 3
    }
}
