using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using CloudContactManager.Services.Interfaces;

namespace CloudContactManager.Services
{
    public class EmailNotificationService : INotificationService
    {
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly IConfiguration _configuration;

        public EmailNotificationService(IAmazonSimpleEmailService sesClient, IConfiguration configuration)
        {
            _sesClient = sesClient;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var sender = _configuration["AWS:SenderEmail"]; // From appsettings.json

            var request = new SendEmailRequest
            {
                Source = sender,
                Destination = new Destination { ToAddresses = new List<string> { toEmail } },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body { Html = new Content(body) }
                }
            };

            try
            {
                await _sesClient.SendEmailAsync(request);
            }
            catch (MessageRejectedException ex)
            {
                // Sender email hasn't been verified (Sandbox mode) or email content blocked
                throw new Exception($"Sandbox Error: Email '{toEmail}' hasn't verified or sender hasn't been verified. Detail: {ex.Message}");
            }
            catch (LimitExceededException ex)
            {
                // Sent too much email in a day or a second (Sending Quota) 
                throw new Exception($"Limit Error: You have pass AWS SES quota. Detail: {ex.Message}");
            }
            catch (AmazonServiceException ex)
            {
                // Other error: Region wrong, Credentials wrong, ...
                throw new Exception($"AWS SES Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Unknow Error: {ex.Message}");
            }
        }
    }
}