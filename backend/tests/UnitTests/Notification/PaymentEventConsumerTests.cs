using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CommerceHub.Modules.Notification.Presentation.Consumers;
using CommerceHub.Modules.Notification.Application.Common.Interfaces;
using CommerceHub.Modules.Notification.Infrastructure.Hubs;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Modules.Notification.UnitTests;

public class PaymentEventConsumerTests
{
    private readonly Mock<IHubContext<NotificationHub>> _hubMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<ILogger<PaymentEventConsumer>> _loggerMock;
    private readonly PaymentEventConsumer _consumer;

    public PaymentEventConsumerTests()
    {
        _hubMock = new Mock<IHubContext<NotificationHub>>();
        _emailMock = new Mock<IEmailService>();
        _userLookupMock = new Mock<IUserLookupService>();
        _loggerMock = new Mock<ILogger<PaymentEventConsumer>>();

        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        _hubMock.Setup(h => h.Clients).Returns(mockClients.Object);

        _consumer = new PaymentEventConsumer(_hubMock.Object, _emailMock.Object, _userLookupMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_PaymentConfirmed_ShouldSendNotification()
    {
        var context = Mock.Of<ConsumeContext<PaymentConfirmed>>(c =>
            c.Message == new PaymentConfirmed
            {
                PaymentId = 1,
                OrderId = 1,
                UserId = 42,
                Amount = 150.00m,
                TransactionId = "txn_123",
                PaymentMethod = "Credit Card",
                ConfirmedAt = DateTime.UtcNow
            });

        await _consumer.Consume(context);

        _hubMock.Verify(h => h.Clients.Group("user_42"), Times.Once);
    }

    [Fact]
    public async Task Consume_PaymentFailed_ShouldSendNotification()
    {
        var context = Mock.Of<ConsumeContext<PaymentFailed>>(c =>
            c.Message == new PaymentFailed
            {
                PaymentId = 1,
                OrderId = 1,
                UserId = 42,
                FailureReason = "Insufficient funds",
                FailedAt = DateTime.UtcNow
            });

        await _consumer.Consume(context);

        _hubMock.Verify(h => h.Clients.Group("user_42"), Times.Once);
    }

    [Fact]
    public async Task Consume_PaymentFailed_ShouldLogWarning()
    {
        var context = Mock.Of<ConsumeContext<PaymentFailed>>(c =>
            c.Message == new PaymentFailed
            {
                PaymentId = 1,
                OrderId = 1,
                UserId = 42,
                FailureReason = "Card declined"
            });

        await _consumer.Consume(context);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Card declined")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Consume_PaymentConfirmed_ShouldLogInformation()
    {
        var context = Mock.Of<ConsumeContext<PaymentConfirmed>>(c =>
            c.Message == new PaymentConfirmed
            {
                PaymentId = 1,
                OrderId = 1,
                UserId = 42,
                Amount = 200m,
                TransactionId = "txn_456",
                PaymentMethod = "PayPal",
                ConfirmedAt = DateTime.UtcNow
            });

        await _consumer.Consume(context);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Payment 1")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
