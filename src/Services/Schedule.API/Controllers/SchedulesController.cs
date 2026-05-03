using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedule.API.Commands;
using Schedule.API.Queries;

namespace Schedule.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SchedulesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SchedulesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Policy = "DoctorOnly")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleCommand command)
        {
            var scheduleId = await _mediator.Send(command);
            return Ok(new { scheduleId, message = "Schedule created successfully" });
        }

        [HttpGet("doctor/{doctorId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctorSchedules(Guid doctorId)
        {
            var schedules = await _mediator.Send(new GetDoctorSchedulesQuery(doctorId));
            return Ok(schedules);
        }
    }
}
