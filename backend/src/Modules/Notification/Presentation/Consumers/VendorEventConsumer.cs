using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using CommerceHub.Modules.Notification.Infrastructure.Hubs;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Modules.Notification.Presentation.Consumers;

public class VendorEventConsumer : IConsumer<VendorPayoutCompleted>, IConsumer<VendorSettled>
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ILogger<VendorEventConsumer> _logger;

    public VendorEventConsumer(IHubContext<NotificationHub> hub, ILogger<VendorEventConsumer> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VendorPayoutCompleted> context)
    {
        _logger.LogInformation("Payout {PayoutNumber} of {Amount} completed for vendor {VendorId}", context.Message.PayoutNumber, context.Message.Amount, context.Message.VendorId);
        await _hub.Clients.Group($"vendor_{context.Message.VendorId}").SendAsync("Notification", new
        {
            Type = "vendor.payout",
            Title = "Payout Completed",
            Message = $"Payout of {context.Message.Amount:C} has been processed.",
            context.Message.VendorId
        });
    }

    public async Task Consume(ConsumeContext<VendorSettled> context)
    {
        _logger.LogInformation("Settlement of {Amount} completed for vendor {VendorId}", context.Message.Amount, context.Message.VendorId);
        await _hub.Clients.Group($"vendor_{context.Message.VendorId}").SendAsync("Notification", new
        {
            Type = "vendor.settled",
            Title = "Settlement Completed",
            Message = $"Settlement of {context.Message.Amount:C} completed.",
            context.Message.VendorId
        });
    }
}
