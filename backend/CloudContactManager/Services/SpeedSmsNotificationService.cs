using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using CloudContactManager.Services.Interfaces;
using CloudContactManager.Services.API;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;

namespace CloudContactManager.Services
{
    /// <summary>
    /// Notification service implementation that sends Email via AWS SES
    /// and SMS via SpeedSMS HTTP API, following the V4 logic.
    /// </summary>
    public class SpeedSmsNotificationService : INotificationService
    {
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SpeedSmsNotificationService> _logger;

        public SpeedSmsNotificationService(
            IAmazonSimpleEmailService sesClient,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<SpeedSmsNotificationService> logger)
        {
            _sesClient = sesClient;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // ================= EMAIL (AWS SES) =================
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var sender = _configuration["AWS:SenderEmail"];

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
                        Html = new Content(body)
                    }
                }
            };

            try
            {
                await _sesClient.SendEmailAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Email Error: {ex.Message}");
            }
        }

        // ================= SMS (SpeedSMS via Speedsmsapi SDK) =================
        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            string token = _configuration["SpeedSMS:AccessToken"];
            string sender = _configuration["SpeedSMS:Device"];

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(sender))
            {
                _logger.LogWarning("SpeedSMS configuration is missing. Skipping SMS send.");
                return;
            }

            try
            {
                var api = new Speedsmsapi(token);
                string[] phones = new[] { phoneNumber };
                int type = Speedsmsapi.TYPE_GATEWAY;

                string response = api.sendSMS(phones, message, type, sender);
                await Task.CompletedTask;

                _logger.LogInformation("SpeedSMS send response: {Response}", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SpeedSMS send failed.");
            }
        }

        // ================= BULK =================
        public async Task SendBulkAsync(List<string> recipients, string message, string type)
        {
            if (type.Equals("Email", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var email in recipients)
                {
                    await SendEmailAsync(email, "Notification", message);
                }
            }
            else if (type.Equals("SMS", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var phone in recipients)
                {
                    await SendSmsAsync(phone, message);
                }
            }
        }
    }
}
