using MassTransit;
using Microsoft.AspNetCore.SignalR;
using CommerceHub.Notification.Infrastructure.Hubs;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Notification.Api.Consumers;

public class OrderEventConsumer : IConsumer<OrderPlaced>, IConsumer<OrderConfirmed>, IConsumer<OrderShipped>, IConsumer<OrderDelivered>, IConsumer<OrderCancelled>
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ILogger<OrderEventConsumer> _logger;

    public OrderEventConsumer(IHubContext<NotificationHub> hub, ILogger<OrderEventConsumer> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        _logger.LogInformation("Order {OrderNumber} placed by user {UserId}", context.Message.OrderNumber, context.Message.UserId);
        await _hub.Clients.Group($"user_{context.Message.UserId}").SendAsync("Notification", new
        {
            Type = "order.placed",
            Title = "Order Placed",
            Message = $"Order {context.Message.OrderNumber} has been placed successfully.",
            context.Message.OrderId,
            context.Message.OrderNumber
        });
    }

    public async Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        _logger.LogInformation("Order {OrderNumber} confirmed", context.Message.OrderNumber);
        await _hub.Clients.Group($"user_{context.Message.UserId}").SendAsync("Notification", new
        {
            Type = "order.confirmed",
            Title = "Order Confirmed",
            Message = $"Order {context.Message.OrderNumber} is confirmed.",
            context.Message.OrderId,
            context.Message.OrderNumber
        });
    }

    public async Task Consume(ConsumeContext<OrderShipped> context)
    {
        await _hub.Clients.Group($"user_{context.Message.UserId}").SendAsync("Notification", new
        {
            Type = "order.shipped",
            Title = "Order Shipped",
            Message = $"Order {context.Message.OrderNumber} has been shipped.",
            TrackingNumber = context.Message.TrackingNumber,
            context.Message.OrderId
        });
    }

    public async Task Consume(ConsumeContext<OrderDelivered> context)
    {
        await _hub.Clients.Group($"user_{context.Message.UserId}").SendAsync("Notification", new
        {
            Type = "order.delivered",
            Title = "Order Delivered",
            Message = $"Order {context.Message.OrderNumber} has been delivered.",
            context.Message.OrderId
        });
    }

    public async Task Consume(ConsumeContext<OrderCancelled> context)
    {
        await _hub.Clients.Group($"user_{context.Message.UserId}").SendAsync("Notification", new
        {
            Type = "order.cancelled",
            Title = "Order Cancelled",
            Message = $"Order {context.Message.OrderNumber} has been cancelled.",
            Reason = context.Message.Reason,
            context.Message.OrderId
        });
    }
}
