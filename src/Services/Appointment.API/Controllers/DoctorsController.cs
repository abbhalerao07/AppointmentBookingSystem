using Appointment.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Appointment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly IScheduleApiService _scheduleApiService;

        public DoctorsController(IScheduleApiService scheduleApiService)
        {
            _scheduleApiService = scheduleApiService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _scheduleApiService.GetDoctorsAsync();
            return Ok(doctors);
        }
    }
}
