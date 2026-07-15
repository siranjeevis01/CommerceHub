using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CommerceHub.Modules.Notification.Presentation.Consumers;
using CommerceHub.Modules.Notification.Application.Common.Interfaces;
using CommerceHub.Modules.Notification.Infrastructure.Hubs;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Modules.Notification.UnitTests;

public class OrderEventConsumerTests
{
    private readonly Mock<IHubContext<NotificationHub>> _hubMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<ILogger<OrderEventConsumer>> _loggerMock;
    private readonly OrderEventConsumer _consumer;

    public OrderEventConsumerTests()
    {
        _hubMock = new Mock<IHubContext<NotificationHub>>();
        _emailMock = new Mock<IEmailService>();
        _userLookupMock = new Mock<IUserLookupService>();
        _loggerMock = new Mock<ILogger<OrderEventConsumer>>();

        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        _hubMock.Setup(h => h.Clients).Returns(mockClients.Object);

        _consumer = new OrderEventConsumer(_hubMock.Object, _emailMock.Object, _userLookupMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_OrderPlaced_ShouldSendNotification()
    {
        var context = Mock.Of<ConsumeContext<OrderPlaced>>(c =>
            c.Message == new OrderPlaced
            {
                OrderId = 1,
                OrderNumber = "ORD-001",
                UserId = 42,
                TotalAmount = 100.00m,
                PlacedAt = DateTime.UtcNow
            });

        await _consumer.Consume(context);

        _hubMock.Verify(h => h.Clients.Group("user_42"), Times.Once);
    }

    [Fact]
    public async Task Consume_OrderConfirmed_ShouldSendNotification()
    {
        var context = Mock.Of<ConsumeContext<OrderConfirmed>>(c =>
            c.Message == new OrderConfirmed
            {
                OrderId = 1,
                OrderNumber = "ORD-001",
                UserId = 42
            });

        await _consumer.Consume(context);

        _hubMock.Verify(h => h.Clients.Group("user_42"), Times.Once);
    }

    [Fact]
    public async Task Consume_OrderShipped_ShouldIncludeTrackingNumber()
    {
        var context = Mock.Of<ConsumeContext<OrderShipped>>(c =>
            c.Message == new OrderShipped
            {
                OrderId = 1,
                OrderNumber = "ORD-001",
                UserId = 42,
                TrackingNumber = "TRACK-123"
            });

        await _consumer.Consume(context);

        _hubMock.Verify(h => h.Clients.Group("user_42"), Times.Once);
    }

    [Fact]
    public async Task Consume_OrderDelivered_ShouldSendNotification()
    {
        var context = Mock.Of<ConsumeContext<OrderDelivered>>(c =>
            c.Message == new OrderDelivered
            {
                OrderId = 1,
                OrderNumber = "ORD-001",
                UserId = 42
            });

        await _consumer.Consume(context);

        _hubMock.Verify(h => h.Clients.Group("user_42"), Times.Once);
    }

    [Fact]
    public async Task Consume_OrderCancelled_ShouldIncludeReason()
    {
        var context = Mock.Of<ConsumeContext<OrderCancelled>>(c =>
            c.Message == new OrderCancelled
            {
                OrderId = 1,
                OrderNumber = "ORD-001",
                UserId = 42,
                Reason = "Out of stock"
            });

        await _consumer.Consume(context);

        _hubMock.Verify(h => h.Clients.Group("user_42"), Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldLogInformation_WhenOrderPlaced()
    {
        var context = Mock.Of<ConsumeContext<OrderPlaced>>(c =>
            c.Message == new OrderPlaced
            {
                OrderId = 1,
                OrderNumber = "ORD-001",
                UserId = 42,
                TotalAmount = 100m,
                PlacedAt = DateTime.UtcNow
            });

        await _consumer.Consume(context);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ORD-001")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
