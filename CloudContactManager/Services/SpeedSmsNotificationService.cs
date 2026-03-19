using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using CloudContactManager.Services.API;
using CloudContactManager.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CloudContactManager.Services
{
    /// <summary>
    /// Notification service implementation that sends SMS via SpeedSMS
    /// (Speedsmsapi) and emails via AWS SES.
    /// </summary>
    public class SpeedSmsNotificationService : INotificationService
    {
        private readonly Speedsmsapi _speedSmsApi;
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SpeedSmsNotificationService> _logger;

        public SpeedSmsNotificationService(
            Speedsmsapi speedSmsApi,
            IAmazonSimpleEmailService sesClient,
            IConfiguration configuration,
            ILogger<SpeedSmsNotificationService> logger)
        {
            _speedSmsApi = speedSmsApi;
            _sesClient = sesClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            var ok = await _speedSmsApi.SendSmsAsync(phoneNumber, message);
            if (!ok)
            {
                _logger.LogWarning("SpeedSMS failed to send SMS to {PhoneNumber}", phoneNumber);
            }
            else
            {
                _logger.LogInformation("SpeedSMS sent SMS to {PhoneNumber}", phoneNumber);
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var sender = _configuration["AWS:SenderEmail"];
            if (string.IsNullOrWhiteSpace(sender))
            {
                throw new InvalidOperationException("AWS:SenderEmail is missing in configuration.");
            }

            var request = new SendEmailRequest
            {
                Source = sender,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { toEmail }
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Text = new Content(body),
                        Html = new Content($"<p>{body}</p>")
                    }
                }
            };

            var response = await _sesClient.SendEmailAsync(request);
            _logger.LogInformation("Email sent to {ToEmail}. Message ID: {MessageId}", toEmail, response.MessageId);
        }

        public async Task SendBulkAsync(List<string> recipients, string message, string type)
        {
            foreach (var recipient in recipients)
            {
                if (type.Equals("SMS", StringComparison.OrdinalIgnoreCase))
                {
                    await SendSmsAsync(recipient, message);
                }
                else if (type.Equals("Email", StringComparison.OrdinalIgnoreCase))
                {
                    await SendEmailAsync(recipient, "Cloud Contact Notification", message);
                }
            }
        }
    }
}
