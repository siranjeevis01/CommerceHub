using MassTransit;
using Microsoft.AspNetCore.SignalR;
using CommerceHub.Notification.Infrastructure.Hubs;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Notification.Api.Consumers;

public class PaymentEventConsumer : IConsumer<PaymentConfirmed>, IConsumer<PaymentFailed>
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ILogger<PaymentEventConsumer> _logger;

    public PaymentEventConsumer(IHubContext<NotificationHub> hub, ILogger<PaymentEventConsumer> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentConfirmed> context)
    {
        _logger.LogInformation("Payment {PaymentId} confirmed for order {OrderId}", context.Message.PaymentId, context.Message.OrderId);
        await _hub.Clients.Group($"user_{context.Message.UserId}").SendAsync("Notification", new
        {
            Type = "payment.confirmed",
            Title = "Payment Confirmed",
            Message = $"Payment of {context.Message.Amount:C} confirmed.",
            context.Message.OrderId
        });
    }

    public async Task Consume(ConsumeContext<PaymentFailed> context)
    {
        _logger.LogWarning("Payment failed for order {OrderId}: {FailureReason}", context.Message.OrderId, context.Message.FailureReason);
        await _hub.Clients.Group($"user_{context.Message.UserId}").SendAsync("Notification", new
        {
            Type = "payment.failed",
            Title = "Payment Failed",
            Message = $"Payment failed: {context.Message.FailureReason}",
            context.Message.OrderId
        });
    }
}
