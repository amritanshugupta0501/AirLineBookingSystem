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
            _logger.LogInformation("Sending booking confirmation email to {Email} for PNR {Pnr}", evt.PassengerEmail, evt.Pnr);

            var msg = new SendGridMessage
            {
                From        = new EmailAddress(_fromEmail, _fromName),
                Subject     = $"Booking Confirmed — PNR: {evt.Pnr}",
                PlainTextContent = BuildPlainText(evt),
                HtmlContent     = BuildHtml(evt)
            };
            msg.AddTo(new EmailAddress(evt.PassengerEmail, evt.PassengerName));

            var response = await _sendGridClient.SendEmailAsync(msg);

            if ((int)response.StatusCode >= 400)
            {
                _logger.LogError("SendGrid returned {StatusCode} for PNR {Pnr}", response.StatusCode, evt.Pnr);
            }
            else
            {
                _logger.LogInformation("Confirmation email sent successfully for PNR {Pnr}", evt.Pnr);
            }
        }

        private static string BuildPlainText(BookingConfirmedEvent e) =>
            $"Dear {e.PassengerName},\n\n" +
            $"Your booking is confirmed!\n\n" +
            $"PNR:          {e.Pnr}\n" +
            $"Flight:       {e.FlightNumber}\n" +
            $"Seat:         {e.SeatIdentifier}\n" +
            $"Amount Paid:  ₹{e.TotalAmount:N2}\n" +
            $"Booked At:    {e.BookingTime:dd MMM yyyy HH:mm} UTC\n\n" +
            "Thank you for flying with us!";

        private static string BuildHtml(BookingConfirmedEvent e) =>
            $"""
            <h2>Booking Confirmed 🎉</h2>
            <p>Dear <strong>{e.PassengerName}</strong>,</p>
            <table>
              <tr><td><b>PNR</b></td><td>{e.Pnr}</td></tr>
              <tr><td><b>Flight</b></td><td>{e.FlightNumber}</td></tr>
              <tr><td><b>Seat</b></td><td>{e.SeatIdentifier}</td></tr>
              <tr><td><b>Amount Paid</b></td><td>₹{e.TotalAmount:N2}</td></tr>
              <tr><td><b>Booked At</b></td><td>{e.BookingTime:dd MMM yyyy HH:mm} UTC</td></tr>
            </table>
            <p>Thank you for flying with us!</p>
            """;
    }
}
