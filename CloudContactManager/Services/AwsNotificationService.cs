using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using CloudContactManager.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace CloudContactManager.Services
{
    public class AwsNotificationService : INotificationService
    {
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly ILogger<AwsNotificationService> _logger;

        public AwsNotificationService(IAmazonSimpleNotificationService snsClient, ILogger<AwsNotificationService> logger)
        {
            _snsClient = snsClient;
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
            _logger.LogInformation($"Email simulation to {toEmail} with subject: {subject}");
        }

        public async Task SendBulkAsync(List<string> recipients, string message, string type)
        {
            foreach (var recipient in recipients)
            {
                if (type == "SMS")
                {
                    await SendSmsAsync(recipient, message);
                }
            }
        }
    }
}