namespace Schedule.API.Models
{
    public class Doctor
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } 
        public string Name { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public List<DoctorSchedule> Schedules { get; set; } = new();
    }
}
