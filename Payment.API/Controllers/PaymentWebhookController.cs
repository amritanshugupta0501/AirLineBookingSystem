using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System;
using Razorpay.Api.Errors;
using MassTransit;
using Shared.Messages;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentWebhookController : ControllerBase
    {
        private readonly string _keyId;
        private readonly string _keySecret;
        private readonly string _webhookSecret;
        private readonly IPublishEndpoint _publishEndpoint;

        public PaymentWebhookController(IConfiguration configuration, IPublishEndpoint publishEndpoint)
        {
            _keyId          = configuration["Razorpay:KeyId"]          ?? throw new InvalidOperationException("Razorpay:KeyId not configured");
            _keySecret      = configuration["Razorpay:KeySecret"]      ?? throw new InvalidOperationException("Razorpay:KeySecret not configured");
            _webhookSecret  = configuration["Razorpay:WebhookSecret"]  ?? throw new InvalidOperationException("Razorpay:WebhookSecret not configured");
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost("razorpay-webhook")]
        public async Task<IActionResult> RazorpayWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["X-Razorpay-Signature"].ToString();

            if (string.IsNullOrEmpty(signature))
                return BadRequest("Signature missing");

            try
            {
                // Verify Webhook Signature securely locally using Razorpay's math validation
                Utils.verifyWebhookSignature(json, signature, _webhookSecret);

                // Parse the JSON payload natively
                using JsonDocument doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                string eventName = root.GetProperty("event").GetString() ?? "";

                // We listen specifically for 'order.paid' or 'payment.captured'
                if (eventName == "order.paid" || eventName == "payment.captured")
                {
                    var paymentEntity = root.GetProperty("payload").GetProperty("payment").GetProperty("entity");
                    
                    // Razorpay amounts are passed in paise (smallest currency unit), divide by 100 for true value
                    decimal amountPaid = paymentEntity.GetProperty("amount").GetDecimal() / 100m;
                    string userEmail = paymentEntity.GetProperty("email").GetString() ?? "passenger@example.com";
                    
                    // Retrieve metadata passed during order creation (e.g., PNR and Email saved in 'notes' in Razorpay)
                    string metadataPnr = "UNKNOWN_PNR";
                    if (paymentEntity.TryGetProperty("notes", out JsonElement notes) && notes.TryGetProperty("pnr", out JsonElement pnrProp))
                    {
                        metadataPnr = pnrProp.GetString() ?? "UNKNOWN_PNR";
                    }

                    // Publish Event to RabbitMQ
                    await _publishEndpoint.Publish(new BookingConfirmedEvent 
                    { 
                        PNR = metadataPnr, 
                        PassengerEmail = userEmail, 
                        AmountPaid = amountPaid 
                    });

                }

                return Ok();
            }
            catch (SignatureVerificationError)
            {
                return BadRequest("Invalid Razorpay Webhook Signature");
            }
            catch (Exception)
            {
                return BadRequest("Failed to process Webhook structure");
            }
        }
    }
}
