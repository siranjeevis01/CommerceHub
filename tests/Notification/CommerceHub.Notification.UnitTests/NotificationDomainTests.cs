using CommerceHub.Notification.Domain.Models;
using FluentAssertions;
using Xunit;

namespace CommerceHub.Notification.UnitTests;

public class NotificationDomainTests
{
    [Fact]
    public void NotificationMessage_ShouldSetProperties()
    {
        var notification = new NotificationMessage
        {
            Title = "Order Confirmed",
            Message = "Your order #ORD-123 has been confirmed.",
            Type = "OrderConfirmed",
            ImageUrl = "https://example.com/icon.png",
            LinkUrl = "/orders/123",
            Data = "{\"orderId\":123}",
            CreatedAt = DateTime.UtcNow
        };

        notification.Type.Should().Be("OrderConfirmed");
        notification.Title.Should().Be("Order Confirmed");
    }

    [Fact]
    public void NotificationMessage_ShouldHaveDefaultTimestamp()
    {
        var notification = new NotificationMessage();
        notification.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void NotificationMessage_ShouldCarryOptionalLink()
    {
        var notification = new NotificationMessage
        {
            Title = "New Sale",
            Message = "Your product just sold!",
            Type = "SaleAlert",
            LinkUrl = "/vendor/orders/456"
        };

        notification.LinkUrl.Should().Be("/vendor/orders/456");
    }

    [Fact]
    public void NotificationMessage_ShouldHandleUserId()
    {
        var notification = new NotificationMessage
        {
            UserId = 42,
            Title = "Welcome",
            Message = "Welcome to CommerceHub!"
        };

        notification.UserId.Should().Be(42);
    }
}
