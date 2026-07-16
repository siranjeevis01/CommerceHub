using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using CommerceHub.Modules.Notification.Application.Common.Interfaces;
using CommerceHub.Modules.Notification.Infrastructure.Hubs;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Modules.Notification.Presentation.Consumers;

public class OrderEventConsumer : IConsumer<OrderPlaced>, IConsumer<OrderConfirmed>, IConsumer<OrderShipped>, IConsumer<OrderDelivered>, IConsumer<OrderCancelled>
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly IEmailService _emailService;
    private readonly IUserLookupService _userLookup;
    private readonly ILogger<OrderEventConsumer> _logger;

    public OrderEventConsumer(IHubContext<NotificationHub> hub, IEmailService emailService, IUserLookupService userLookup, ILogger<OrderEventConsumer> logger)
    {
        _hub = hub;
        _emailService = emailService;
        _userLookup = userLookup;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Order {OrderNumber} placed by user {UserId}", msg.OrderNumber, msg.UserId);
        await _hub.Clients.Group($"user_{msg.UserId}").SendAsync("Notification", new
        {
            Type = "order.placed",
            Title = "Order Placed",
            Message = $"Order {msg.OrderNumber} has been placed successfully.",
            msg.OrderId,
            msg.OrderNumber
        });

        var user = await _userLookup.GetUserAsync(msg.UserId, context.CancellationToken);
        if (user != null)
        {
            await _emailService.SendEmailWithTemplateAsync(user.Email, "order.confirmed", new Dictionary<string, string>
            {
                ["OrderNumber"] = msg.OrderNumber,
                ["TotalAmount"] = msg.TotalAmount.ToString("C"),
                ["CustomerName"] = user.FullName
            }, context.CancellationToken);
        }
    }

    public async Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Order {OrderNumber} confirmed", msg.OrderNumber);
        await _hub.Clients.Group($"user_{msg.UserId}").SendAsync("Notification", new
        {
            Type = "order.confirmed",
            Title = "Order Confirmed",
            Message = $"Order {msg.OrderNumber} is confirmed.",
            msg.OrderId,
            msg.OrderNumber
        });
    }

    public async Task Consume(ConsumeContext<OrderShipped> context)
    {
        var msg = context.Message;
        await _hub.Clients.Group($"user_{msg.UserId}").SendAsync("Notification", new
        {
            Type = "order.shipped",
            Title = "Order Shipped",
            Message = $"Order {msg.OrderNumber} has been shipped.",
            TrackingNumber = msg.TrackingNumber,
            msg.OrderId
        });

        var user = await _userLookup.GetUserAsync(msg.UserId, context.CancellationToken);
        if (user != null)
        {
            await _emailService.SendEmailWithTemplateAsync(user.Email, "order.shipped", new Dictionary<string, string>
            {
                ["OrderNumber"] = msg.OrderNumber,
                ["TrackingNumber"] = msg.TrackingNumber ?? "N/A",
                ["CustomerName"] = user.FullName
            }, context.CancellationToken);
        }
    }

    public async Task Consume(ConsumeContext<OrderDelivered> context)
    {
        var msg = context.Message;
        await _hub.Clients.Group($"user_{msg.UserId}").SendAsync("Notification", new
        {
            Type = "order.delivered",
            Title = "Order Delivered",
            Message = $"Order {msg.OrderNumber} has been delivered.",
            msg.OrderId
        });

        var user = await _userLookup.GetUserAsync(msg.UserId, context.CancellationToken);
        if (user != null)
        {
            await _emailService.SendEmailWithTemplateAsync(user.Email, "order.delivered", new Dictionary<string, string>
            {
                ["OrderNumber"] = msg.OrderNumber,
                ["CustomerName"] = user.FullName
            }, context.CancellationToken);
        }
    }

    public async Task Consume(ConsumeContext<OrderCancelled> context)
    {
        var msg = context.Message;
        await _hub.Clients.Group($"user_{msg.UserId}").SendAsync("Notification", new
        {
            Type = "order.cancelled",
            Title = "Order Cancelled",
            Message = $"Order {msg.OrderNumber} has been cancelled.",
            Reason = msg.Reason,
            msg.OrderId
        });

        var user = await _userLookup.GetUserAsync(msg.UserId, context.CancellationToken);
        if (user != null)
        {
            await _emailService.SendEmailWithTemplateAsync(user.Email, "order.cancelled", new Dictionary<string, string>
            {
                ["OrderNumber"] = msg.OrderNumber,
                ["Reason"] = msg.Reason ?? "No reason provided",
                ["CustomerName"] = user.FullName
            }, context.CancellationToken);
        }
    }
}
