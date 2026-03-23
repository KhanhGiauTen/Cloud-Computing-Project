using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using CloudContactManager.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CloudContactManager.Tests;

public class AwsNotificationServiceTests
{
    [Fact]
    public async Task SendSmsAsync_ShouldCallPublishAsync_Once()
    {
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        snsMock
            .Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PublishResponse { MessageId = "msg-1" });

        var sesMock = new Mock<IAmazonSimpleEmailService>();
        var loggerMock = new Mock<ILogger<AwsNotificationService>>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AWS:SenderEmail"] = "verified-sender@example.com"
            })
            .Build();

        var service = new AwsNotificationService(snsMock.Object, sesMock.Object, configuration, loggerMock.Object);

        await service.SendSmsAsync("+84901234567", "hello");

        snsMock.Verify(
            x => x.PublishAsync(
                It.Is<PublishRequest>(r => r.PhoneNumber == "+84901234567" && r.Message == "hello"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_ShouldCallSesSendEmailAsync_Once()
    {
        var snsMock = new Mock<IAmazonSimpleNotificationService>();
        var sesMock = new Mock<IAmazonSimpleEmailService>();
        sesMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SendEmailResponse { MessageId = "email-1" });

        var loggerMock = new Mock<ILogger<AwsNotificationService>>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AWS:SenderEmail"] = "verified-sender@example.com"
            })
            .Build();

        var service = new AwsNotificationService(snsMock.Object, sesMock.Object, configuration, loggerMock.Object);

        await service.SendEmailAsync("receiver@example.com", "subject", "body");

        sesMock.Verify(
            x => x.SendEmailAsync(
                It.Is<SendEmailRequest>(r =>
                    r.Source == "verified-sender@example.com"
                    && r.Destination.ToAddresses.Contains("receiver@example.com")
                    && r.Message.Subject.Data == "subject"
                    && r.Message.Body.Text.Data == "body"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
