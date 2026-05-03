namespace Availability.API.Models
{
    public class BookedSlot
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
