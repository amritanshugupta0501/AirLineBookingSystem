using MassTransit;
using Shared.Messages;
using Microsoft.Extensions.Logging;

namespace Notification.Worker
{
    public class PaymentConfirmedConsumer : IConsumer<PaymentConfirmedEvent>
    {
        private readonly ILogger<PaymentConfirmedConsumer> _logger;

        public PaymentConfirmedConsumer(ILogger<PaymentConfirmedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<PaymentConfirmedEvent> context)
        {
            var message = context.Message;
            
            _logger.LogInformation("=========================================");
            _logger.LogInformation("🔔 NOTIFICATION QUEUE FIRED 🔔");
            _logger.LogInformation("=========================================");
            _logger.LogInformation($"Booking Confirmed! Sending Email/SMS to user.");
            _logger.LogInformation($"To: {message.PassengerEmail}");
            _logger.LogInformation($"PNR: {message.Pnr}");
            _logger.LogInformation($"Total Paid: ${message.AmountPaid}");
            _logger.LogInformation($"Time: {message.ConfirmationTime}");
            _logger.LogInformation("=========================================");

            return Task.CompletedTask;
        }
    }
}
