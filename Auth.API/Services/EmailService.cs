using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Auth.API.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otp);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var smtpHost = _config["Smtp:Host"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_config["Smtp:Port"] ?? "587");
            var smtpUser = _config["Smtp:Username"] ?? "";
            var smtpPass = _config["Smtp:Password"] ?? "";
            var fromName = _config["Smtp:FromName"] ?? "AeroFlight";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, smtpUser));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Your AeroFlight Staff OTP";

            message.Body = new TextPart("html")
            {
                Text = $"""
                    <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto; background: #0f172a; color: #e2e8f0; padding: 32px; border-radius: 16px;">
                        <div style="text-align: center; margin-bottom: 24px;">
                            <span style="font-size: 48px;">✈️</span>
                            <h1 style="color: #6366f1; margin: 8px 0;">AeroFlight</h1>
                            <p style="color: #94a3b8;">Staff Portal Registration</p>
                        </div>
                        <div style="background: #1e293b; border-radius: 12px; padding: 24px; text-align: center; margin-bottom: 24px;">
                            <p style="margin: 0 0 12px 0; color: #94a3b8;">Your one-time password (OTP) is:</p>
                            <div style="font-size: 40px; font-weight: 700; letter-spacing: 12px; color: #6366f1; padding: 16px; background: rgba(99,102,241,0.1); border-radius: 8px;">
                                {otp}
                            </div>
                            <p style="margin: 12px 0 0 0; color: #64748b; font-size: 13px;">This OTP expires in <strong>10 minutes</strong>.</p>
                        </div>
                        <p style="color: #64748b; font-size: 12px; text-align: center;">If you did not request this, please ignore this email.</p>
                    </div>
                """
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
