using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace CloudContactManager.Services
{
    public class AwsNotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AwsNotificationService> _logger;
        private readonly RegionEndpoint _region;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _senderEmail;

        public AwsNotificationService(IConfiguration configuration, ILogger<AwsNotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Load AWS configuration from appsettings.json
            var awsSection = _configuration.GetSection("AWS");
            _region = RegionEndpoint.GetBySystemName(awsSection["Region"] ?? "us-east-1");
            _accessKey = awsSection["AccessKey"] ?? string.Empty;
            _secretKey = awsSection["SecretKey"] ?? string.Empty;
            _senderEmail = awsSection["SenderEmail"] ?? "noreply@example.com";
        }

        /// <inheritdoc />
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new AmazonSimpleEmailServiceClient(_accessKey, _secretKey, _region);

                var sendRequest = new SendEmailRequest
                {
                    Source = _senderEmail,
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { toEmail }
                    },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = "UTF-8",
                                Data = body
                            },
                            Text = new Content
                            {
                                Charset = "UTF-8",
                                Data = body
                            }
                        }
                    }
                };

                var response = await client.SendEmailAsync(sendRequest);

                _logger.LogInformation("Email sent successfully to {Email}. MessageId: {MessageId}", 
                    toEmail, response.MessageId);

                return true;
            }
            catch (MessageRejectedException ex)
            {
                // This often occurs in AWS SES Sandbox mode when email is not verified
                _logger.LogWarning("Email rejected (SES Sandbox limitation): {Message}. " +
                    "Ensure the recipient email {Email} is verified in SES Sandbox.", 
                    ex.Message, toEmail);
                return false;
            }
            catch (AmazonSimpleEmailServiceException ex)
            {
                _logger.LogError(ex, "AWS SES error while sending email to {Email}: {ErrorCode} - {Message}", 
                    toEmail, ex.ErrorCode, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending email to {Email}", toEmail);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                using var client = new AmazonSimpleNotificationServiceClient(_accessKey, _secretKey, _region);

                var publishRequest = new PublishRequest
                {
                    PhoneNumber = phoneNumber,
                    Message = message,
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>
                    {
                        {
                            "AWS.SNS.SMS.SMSType",
                            new MessageAttributeValue
                            {
                                StringValue = "Transactional",
                                DataType = "String"
                            }
                        }
                    }
                };

                var response = await client.PublishAsync(publishRequest);

                _logger.LogInformation("SMS sent successfully to {PhoneNumber}. MessageId: {MessageId}", 
                    phoneNumber, response.MessageId);

                return true;
            }
            catch (AmazonSimpleNotificationServiceException ex)
            {
                // SNS Sandbox mode may have limitations on sending SMS
                _logger.LogWarning("AWS SNS error while sending SMS to {PhoneNumber}: {ErrorCode} - {Message}. " +
                    "This may be due to SNS Sandbox limitations.", 
                    phoneNumber, ex.ErrorCode, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending SMS to {PhoneNumber}", phoneNumber);
                return false;
            }
        }
    }
}
