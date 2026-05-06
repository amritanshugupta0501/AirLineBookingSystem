using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Messages;
using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Notification.Worker.Consumers
{
    public class SmsConsumer : IConsumer<BookingConfirmedEvent>
    {
        private readonly ILogger<SmsConsumer> _logger;
        private readonly string _fromNumber;

        public SmsConsumer(ILogger<SmsConsumer> logger, IConfiguration config)
        {
            _logger = logger;
            var accountSid = config["Twilio:AccountSid"] ?? throw new InvalidOperationException("Twilio:AccountSid not configured");
            var authToken  = config["Twilio:AuthToken"]  ?? throw new InvalidOperationException("Twilio:AuthToken not configured");
            _fromNumber    = config["Twilio:FromNumber"] ?? throw new InvalidOperationException("Twilio:FromNumber not configured");
            TwilioClient.Init(accountSid, authToken);
        }

        public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
        {
            var evt = context.Message;
            string passengerPhone = "+1234567890"; // Mock phone number since it's not in the event
            _logger.LogInformation("Sending SMS to {Phone} for PNR {Pnr}", passengerPhone, evt.PNR);

            var message = await MessageResource.CreateAsync(
                body: $"[Airline] Booking confirmed! PNR: {evt.PNR} | ₹{evt.AmountPaid:N0}",
                from: new Twilio.Types.PhoneNumber(_fromNumber),
                to:   new Twilio.Types.PhoneNumber(passengerPhone)
            );

            _logger.LogInformation("SMS sent — Twilio SID: {Sid}", message.Sid);
        }
    }
}
