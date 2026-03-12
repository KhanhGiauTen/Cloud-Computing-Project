using CloudContactManager.Services.Interfaces;

namespace CloudContactManager.Services
{
    public class LocalNotificationService : INotificationService
    {
        private readonly ILogger<LocalNotificationService> _logger;

        public LocalNotificationService(ILogger<LocalNotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendSmsAsync(string phoneNumber, string message)
        {
            _logger.LogInformation("[LOCAL SMS] To: {Phone} | Content: {Message}", phoneNumber, message);
            Console.WriteLine($"[LOCAL SMS] To: {phoneNumber} | Content: {message}");
            return Task.CompletedTask;
        }

        public Task SendEmailAsync(string toEmail, string subject, string body)
        {
            _logger.LogInformation("[LOCAL EMAIL] To: {Email} | Subject: {Subject} | Body: {Body}", toEmail, subject, body);
            Console.WriteLine($"[LOCAL EMAIL] To: {toEmail} | Subject: {subject}");
            return Task.CompletedTask;
        }

        public async Task SendBulkAsync(List<string> recipients, string message, string type)
        {
            Console.WriteLine($"[LOCAL BULK] Sending {type} to {recipients.Count} recipients...");
            foreach (var recipient in recipients)
            {
                if (type == "SMS")
                    await SendSmsAsync(recipient, message);
                else
                    await SendEmailAsync(recipient, "Notification", message);
            }
            Console.WriteLine($"[LOCAL BULK] Done! Sent to {recipients.Count} recipients.");
        }
    }
}
