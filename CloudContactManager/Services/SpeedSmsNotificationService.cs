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

        public Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                // Gọi SpeedSMS client theo đúng SDK: gửi dạng CSKH (TYPE_CSKH), không dùng brandname
                var json = _speedSmsApi.sendSMS(
                    new[] { phoneNumber },
                    message,
                    Speedsmsapi.TYPE_CSKH,
                    string.Empty);

                var ok = false;
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("status", out var statusProp))
                        {
                            var status = statusProp.GetString();
                            ok = string.Equals(status, "success", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    catch
                    {
                        // Nếu parse JSON lỗi, xem như thất bại
                        ok = false;
                    }
                }

                if (!ok)
                {
                    _logger.LogWarning("SpeedSMS failed to send SMS to {PhoneNumber}. Raw response: {Response}", phoneNumber, json);
                }
                else
                {
                    _logger.LogInformation("SpeedSMS sent SMS to {PhoneNumber}", phoneNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while sending SMS to {PhoneNumber} via SpeedSMS", phoneNumber);
            }

            // API gốc là đồng bộ, nên ta bọc lại thành Task cho interface INotificationService
            return Task.CompletedTask;
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
