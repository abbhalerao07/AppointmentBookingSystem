using Availability.API.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Availability.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorAvailabilityController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DoctorAvailabilityController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("doctor/{doctorId}")]
        [Authorize]
        public async Task<IActionResult> GetAvailability(Guid doctorId, [FromQuery] DateTime date)
        {
            if (doctorId == Guid.Empty)
                return BadRequest(new { message = "DoctorId is required" });

            if (date == default)
                return BadRequest(new { message = "Valid date is required" });

            var result = await _mediator.Send(new GetDoctorAvailabilityQuery(doctorId, date.Date));
            return Ok(result);
        }
    }
}
