using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace CloudContactManager.Services.Interfaces
{
    /// <summary>
    /// Interface for notification services (Email and SMS) using AWS SES/SNS.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends an email asynchronously using AWS SES.
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body content</param>
        Task SendEmailAsync(string toEmail, string subject, string body);

        /// <summary>
        /// Sends an SMS message asynchronously using AWS SNS.
        /// </summary>
        /// <param name="phoneNumber">Recipient phone number (E.164 format)</param>
        /// <param name="message">SMS message content</param>
        Task SendSmsAsync(string phoneNumber, string message);

        /// <summary>
        /// Sends bulk notifications to multiple recipients.
        /// [Requirement: Select multiple customers and send communication]
        /// </summary>
        /// <param name="recipients">List of recipient addresses (emails or phone numbers)</param>
        /// <param name="message">Message content to send</param>
        /// <param name="type">Type of communication: "SMS" or "Email"</param>
        Task SendBulkAsync(List<string> recipients, string message, string type);
    }
}
