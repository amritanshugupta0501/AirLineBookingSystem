using MassTransit;
using Shared.Messages;
using Microsoft.Extensions.Logging;

namespace Notification.Worker.Consumers
{
    public class BookingConfirmedEventConsumer : IConsumer<BookingConfirmedEvent>
    {
        private readonly ILogger<BookingConfirmedEventConsumer> _logger;

        public BookingConfirmedEventConsumer(ILogger<BookingConfirmedEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<BookingConfirmedEvent> context)
        {
            var data = context.Message;
            
            // Simulate sending an email/SMS notification
            _logger.LogInformation("=========================================================");
            _logger.LogInformation("[NOTIFICATION] Email sent to {Email} for PNR {PNR}", data.PassengerEmail, data.PNR);
            _logger.LogInformation("[NOTIFICATION] Amount Paid: {Amount}", data.AmountPaid);
            _logger.LogInformation("=========================================================");

            return Task.CompletedTask;
        }
    }
}
