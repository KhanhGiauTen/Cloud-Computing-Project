namespace CloudContactManager.Services
{
    public interface INotificationService
    {
        /// <summary>
        /// Sends an email asynchronously using AWS SES.
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body content</param>
        /// <returns>True if email was sent successfully, false otherwise</returns>
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);

        /// <summary>
        /// Sends an SMS message asynchronously using AWS SNS.
        /// </summary>
        /// <param name="phoneNumber">Recipient phone number (E.164 format)</param>
        /// <param name="message">SMS message content</param>
        /// <returns>True if SMS was sent successfully, false otherwise</returns>
        Task<bool> SendSmsAsync(string phoneNumber, string message);
    }
}
