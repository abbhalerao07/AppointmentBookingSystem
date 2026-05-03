using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Worker.Services
{
    //only handles email sending
    public interface IEmailService
    {
        Task SendAppointmentConfirmationAsync(
            string toEmail,
            string patientName,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime);

        Task SendAppointmentCancellationAsync(
            string toEmail,
            DateTime appointmentDate,
            string reason);
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendAppointmentConfirmationAsync(
            string toEmail,
            string patientName,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            _logger.LogInformation(
                "EMAIL SENT TO: {Email}\n" +
                "Subject: Appointment Confirmation\n" +
                "Dear {Name},\n" +
                "Your appointment is confirmed for {Date} at {Time}.\n" +
                "Duration: {Start} - {End}",
                toEmail, patientName, appointmentDate.ToShortDateString(),
                startTime.ToString(@"hh\:mm"), startTime, endTime);

            return Task.CompletedTask;
        }

        public Task SendAppointmentCancellationAsync(
            string toEmail,
            DateTime appointmentDate,
            string reason)
        {
            _logger.LogInformation(
                "EMAIL SENT TO: {Email}\n" +
                "Subject: Appointment Cancelled\n" +
                "Your appointment on {Date} has been cancelled.\n" +
                "Reason: {Reason}",
                toEmail, appointmentDate.ToShortDateString(), reason);

            return Task.CompletedTask;
        }
    }
}
