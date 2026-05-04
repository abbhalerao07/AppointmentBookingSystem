
using Appointment.API.Commands;
using Appointment.API.Data;
using Appointment.API.Models;
using Appointment.API.Services;
using Common.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Appointment.API.Handlers
{
    public class CancelAppointmentHandler : IRequestHandler<CancelAppointmentCommand>
    {
        private readonly AppointmentDbContext _context;
        private readonly IRabbitMqPublisher _publisher;

        public CancelAppointmentHandler(AppointmentDbContext context, IRabbitMqPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }

        public async Task Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId
                                       && a.Status != AppointmentStatus.Cancelled);
            if (appointment == null)
                throw new Exception("Appointment not found");

            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync(cancellationToken);

            // Publish cancellation event
            var cancelEvent = new AppointmentCancelledEvent
            {
                AppointmentId = appointment.Id, 
                PatientEmail = "abbhalerao@gmail.com", 
                AppointmentDate = appointment.AppointmentDate,
                Reason = request.Reason ?? "Cancelled by user"
            };

            _publisher.PublishAppointmentCancelled(cancelEvent);
        }
    }
}
