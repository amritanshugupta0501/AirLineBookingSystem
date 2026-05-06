using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Shared.Messages;
using System.Threading.Tasks;

namespace Notification.Worker.Consumers
{
    public class EmailConsumer : IConsumer<BookingConfirmedEvent>
    {
        private readonly ILogger<EmailConsumer> _logger;
        private readonly ISendGridClient _sendGridClient;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailConsumer(ILogger<EmailConsumer> logger, IConfiguration config)
        {
            _logger = logger;
            _fromEmail = config["SendGrid:FromEmail"] ?? "noreply@airline.com";
            _fromName  = config["SendGrid:FromName"]  ?? "Airline Booking";
            _sendGridClient = new SendGridClient(
                config["SendGrid:ApiKey"] ?? throw new System.InvalidOperationException("SendGrid:ApiKey not configured"));
        }

        public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
        {
            var evt = context.Message;
            _logger.LogInformation("Sending booking confirmation email to {Email} for PNR {Pnr}", evt.PassengerEmail, evt.PNR);

            var msg = new SendGridMessage
            {
                From        = new EmailAddress(_fromEmail, _fromName),
                Subject     = $"Booking Confirmed ΓÇö PNR: {evt.PNR}",
                PlainTextContent = BuildPlainText(evt),
                HtmlContent     = BuildHtml(evt)
            };
            msg.AddTo(new EmailAddress(evt.PassengerEmail, "Valued Passenger"));

            var response = await _sendGridClient.SendEmailAsync(msg);

            if ((int)response.StatusCode >= 400)
            {
                _logger.LogError("SendGrid returned {StatusCode} for PNR {Pnr}", response.StatusCode, evt.PNR);
            }
            else
            {
                _logger.LogInformation("Confirmation email sent successfully for PNR {Pnr}", evt.PNR);
            }
        }

        private static string BuildPlainText(BookingConfirmedEvent e) =>
            $"Dear Valued Passenger,\n\n" +
            $"Your booking is confirmed!\n\n" +
            $"PNR:          {e.PNR}\n" +
            $"Amount Paid:  Γé╣{e.AmountPaid:N2}\n" +
            $"Booked At:    {System.DateTime.UtcNow:dd MMM yyyy HH:mm} UTC\n\n" +
            "Thank you for flying with us!";

        private static string BuildHtml(BookingConfirmedEvent e) =>
            $"""
            <h2>Booking Confirmed ≡ƒÄë</h2>
            <p>Dear <strong>Valued Passenger</strong>,</p>
            <table>
              <tr><td><b>PNR</b></td><td>{e.PNR}</td></tr>
              <tr><td><b>Amount Paid</b></td><td>Γé╣{e.AmountPaid:N2}</td></tr>
              <tr><td><b>Booked At</b></td><td>{System.DateTime.UtcNow:dd MMM yyyy HH:mm} UTC</td></tr>
            </table>
            <p>Thank you for flying with us!</p>
            """;
    }
}
