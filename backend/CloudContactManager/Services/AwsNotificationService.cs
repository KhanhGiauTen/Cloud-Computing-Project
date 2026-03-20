using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using CloudContactManager.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace CloudContactManager.Services
{
    public class AwsNotificationService : INotificationService
    {
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AwsNotificationService> _logger;

        public AwsNotificationService(
            IAmazonSimpleNotificationService snsClient,
            IAmazonSimpleEmailService sesClient,
            IConfiguration configuration,
            ILogger<AwsNotificationService> logger)
        {
            _snsClient = snsClient;
            _sesClient = sesClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                var request = new PublishRequest
                {
                    Message = message,
                    PhoneNumber = phoneNumber
                };

                var response = await _snsClient.PublishAsync(request);

                _logger.LogInformation($"SMS sent to {phoneNumber}. Message ID: {response.MessageId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send SMS to {phoneNumber}: {ex.Message}");
                Console.WriteLine($"[SIMULATED SMS] To: {phoneNumber} | Content: {message}");
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

            try
            {
                var response = await _sesClient.SendEmailAsync(request);
                _logger.LogInformation("Email sent to {ToEmail}. Message ID: {MessageId}", toEmail, response.MessageId);
            }
            catch (MessageRejectedException ex)
            {
                _logger.LogError(ex, "SES rejected email to {ToEmail}", toEmail);
                throw;
            }
            catch (AmazonSimpleEmailServiceException ex)
            {
                _logger.LogError(ex, "AWS SES error while sending email to {ToEmail}", toEmail);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending email to {ToEmail}", toEmail);
                throw;
            }
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