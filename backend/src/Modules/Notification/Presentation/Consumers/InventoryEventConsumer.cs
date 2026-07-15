using MassTransit;
using Microsoft.AspNetCore.SignalR;
using CommerceHub.Modules.Notification.Infrastructure.Hubs;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Modules.Notification.Presentation.Consumers;

public class InventoryEventConsumer : IConsumer<LowStockAlert>
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ILogger<InventoryEventConsumer> _logger;

    public InventoryEventConsumer(IHubContext<NotificationHub> hub, ILogger<InventoryEventConsumer> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LowStockAlert> context)
    {
        _logger.LogWarning("Low stock alert: Product {ProductName} has {CurrentStock} remaining", context.Message.ProductName, context.Message.CurrentStock);
        await _hub.Clients.Group("admin").SendAsync("Notification", new
        {
            Type = "inventory.low_stock",
            Title = "Low Stock Alert",
            Message = $"Product '{context.Message.ProductName}' is low on stock ({context.Message.CurrentStock} remaining).",
            context.Message.ProductId
        });
    }
}
