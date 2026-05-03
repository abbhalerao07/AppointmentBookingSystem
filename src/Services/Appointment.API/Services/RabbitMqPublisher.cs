using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Common.Events;

namespace Appointment.API.Services
{
    public interface IRabbitMqPublisher
    {
        void PublishAppointmentBooked(AppointmentBookedEvent eventData);
        void PublishAppointmentCancelled(AppointmentCancelledEvent eventData);
    }

    public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMqPublisher> _logger;

        public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQ:Host"] ?? "localhost",
                UserName = configuration["RabbitMQ:Username"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Queues for booking and availability
            _channel.QueueDeclare(queue: "appointment-booked", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare(queue: "appointment-cancelled", durable: true, exclusive: false, autoDelete: false);

            // Queues for booking and availability
            _channel.QueueDeclare(queue: "appointment-booked-availability", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare(queue: "appointment-cancelled-availability", durable: true, exclusive: false, autoDelete: false);
        }

        public void PublishAppointmentBooked(AppointmentBookedEvent eventData)
        {
            PublishMessage("appointment-booked", eventData);
            PublishMessage("appointment-booked-availability", eventData);
        }

        public void PublishAppointmentCancelled(AppointmentCancelledEvent eventData)
        {
            PublishMessage("appointment-cancelled", eventData);
            PublishMessage("appointment-cancelled-availability", eventData);
        }

        private void PublishMessage(string queueName, object eventData)
        {
            var json = JsonSerializer.Serialize(eventData);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation("Published message to {QueueName}", queueName);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
