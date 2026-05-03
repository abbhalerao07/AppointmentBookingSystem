using Appointment.API.Models;
using System.Text.Json;

namespace Appointment.API.Services
{
    public interface IScheduleApiService
    {
        Task<List<DoctorListItemDto>> GetDoctorsAsync();
    }

    public class ScheduleApiService : IScheduleApiService
    {
        private readonly HttpClient _httpClient;

        public ScheduleApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<DoctorListItemDto>> GetDoctorsAsync()
        {
            var response = await _httpClient.GetAsync("api/doctors");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var doctors = JsonSerializer.Deserialize<List<DoctorListItemDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return doctors ?? new List<DoctorListItemDto>();
        }
    }
}
