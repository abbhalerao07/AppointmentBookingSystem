
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Common.Events;

namespace Notification.Worker.Services
{
    public class RabbitMqConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel; 
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMqConsumer> _logger;

        public RabbitMqConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<RabbitMqConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQ:Host"] ?? "localhost",
                UserName = configuration["RabbitMQ:Username"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel(); 

            // Declare queues
            _channel.QueueDeclare(queue: "appointment-booked", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare(queue: "appointment-cancelled", durable: true, exclusive: false, autoDelete: false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Listen to appointment-booked queue
            var bookedConsumer = new EventingBasicConsumer(_channel);
            bookedConsumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"📨 Received appointment-booked: {message}");

                try
                {
                    var appointmentData = JsonSerializer.Deserialize<AppointmentBookedEvent>(message);

                    if (appointmentData != null)
                    {
                        await SendBookingNotification(appointmentData);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing appointment-booked: {ex.Message}");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            _channel.BasicConsume(queue: "appointment-booked", autoAck: false, consumer: bookedConsumer);

            // Listen to appointment-cancelled queue
            var cancelledConsumer = new EventingBasicConsumer(_channel);
            cancelledConsumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Received appointment-cancelled: {message}");

                try
                {
                    var appointmentData = JsonSerializer.Deserialize<AppointmentCancelledEvent>(message);

                    if (appointmentData != null)
                    {
                        await SendCancellationNotification(appointmentData);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing appointment-cancelled: {ex.Message}");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            _channel.BasicConsume(queue: "appointment-cancelled", autoAck: false, consumer: cancelledConsumer);

            _logger.LogInformation("RabbitMQ Consumer started - listening for events...");
            return Task.CompletedTask;
        }

        private async Task SendBookingNotification(AppointmentBookedEvent eventData)
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            await emailService.SendAppointmentConfirmationAsync(
                toEmail: eventData.PatientEmail,
                patientName: eventData.PatientName,
                appointmentDate: eventData.AppointmentDate,
                startTime: eventData.StartTime,
                endTime: eventData.EndTime
            );
        }

        private async Task SendCancellationNotification(AppointmentCancelledEvent eventData)
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            await emailService.SendAppointmentCancellationAsync(
                toEmail: eventData.PatientEmail,
                appointmentDate: eventData.AppointmentDate,
                reason: eventData.Reason
            );
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
