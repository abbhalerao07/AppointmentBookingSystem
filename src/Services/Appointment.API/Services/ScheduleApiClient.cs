using Appointment.API.Models;

namespace Appointment.API.Services
{
    public interface IScheduleApiClient
    {
        Task<List<ScheduleDto>> GetDoctorSchedulesAsync(Guid doctorId, CancellationToken cancellationToken);
    }

    public class ScheduleApiClient : IScheduleApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ScheduleApiClient> _logger;

        public ScheduleApiClient(HttpClient httpClient, ILogger<ScheduleApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ScheduleDto>> GetDoctorSchedulesAsync(Guid doctorId, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"/api/schedules/doctor/{doctorId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Schedule API returned {StatusCode} for DoctorId: {DoctorId}",
                    response.StatusCode, doctorId);

                return new List<ScheduleDto>();
            }

            var schedules = await response.Content.ReadFromJsonAsync<List<ScheduleDto>>(cancellationToken: cancellationToken);
            return schedules ?? new List<ScheduleDto>();
        }
    }
}
