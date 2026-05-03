using Appointment.API.Commands;
using Appointment.API.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Appointment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AppointmentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentCommand command)
        {
            try
            {
                var appointmentId = await _mediator.Send(command);
                return Ok(new { appointmentId, message = "Appointment booked successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-appointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var appointments = await _mediator.Send(new GetMyAppointmentsQuery());
            return Ok(appointments);
        }

        [HttpGet("doctors-list")]
        public async Task<IActionResult> GetDoctors()
        {
            var appointments = await _mediator.Send(new GetMyAppointmentsQuery());
            return Ok(appointments);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelAppointment(Guid id, [FromBody] CancelRequest request)
        {
            try
            {
                await _mediator.Send(new CancelAppointmentCommand(id, request.Reason));
                return Ok(new { message = "Appointment cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public record CancelRequest(string Reason);
}
