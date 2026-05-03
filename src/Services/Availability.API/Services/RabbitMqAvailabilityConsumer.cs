using System.Text;
using System.Text.Json;
using Availability.API.Data;
using Availability.API.Models;
using Common.Events;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Availability.API.Services
{
    public class RabbitMqAvailabilityConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMqAvailabilityConsumer> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqAvailabilityConsumer(
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ILogger<RabbitMqAvailabilityConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"] ?? "localhost",
                UserName = configuration["RabbitMQ:Username"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare("appointment-booked-availability", true, false, false);
            _channel.QueueDeclare("appointment-cancelled-availability", true, false, false);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var bookedConsumer = new EventingBasicConsumer(_channel);
            bookedConsumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var data = JsonSerializer.Deserialize<AppointmentBookedEvent>(message);

                    if (data != null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<DoctorAvailabilityDbContext>();

                        var exists = await db.BookedSlots.AnyAsync(x => x.AppointmentId == data.AppointmentId);

                        if (!exists)
                        {
                            db.BookedSlots.Add(new BookedSlot
                            {
                                Id = Guid.NewGuid(),
                                AppointmentId = data.AppointmentId,
                                DoctorId = data.DoctorId,
                                AppointmentDate = data.AppointmentDate.Date,
                                StartTime = data.StartTime,
                                EndTime = data.EndTime,
                                IsCancelled = false,
                                CreatedAt = DateTime.UtcNow
                            });

                            await db.SaveChangesAsync();
                        }
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing booked event");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume("appointment-booked-availability", false, bookedConsumer);

            var cancelledConsumer = new EventingBasicConsumer(_channel);
            cancelledConsumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var data = JsonSerializer.Deserialize<AppointmentCancelledEvent>(message);

                    if (data != null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<DoctorAvailabilityDbContext>();

                        var slot = await db.BookedSlots
                            .FirstOrDefaultAsync(x => x.AppointmentId == data.AppointmentId);

                        if (slot != null)
                        {
                            slot.IsCancelled = true;
                            await db.SaveChangesAsync();
                        }
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing cancelled event");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume("appointment-cancelled-availability", false, cancelledConsumer);

            _logger.LogInformation("Availability consumer started");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
