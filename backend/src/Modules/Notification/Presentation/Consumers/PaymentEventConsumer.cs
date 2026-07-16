using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using CommerceHub.Modules.Notification.Application.Common.Interfaces;
using CommerceHub.Modules.Notification.Infrastructure.Hubs;
using CommerceHub.Shared.Contracts.Events;

namespace CommerceHub.Modules.Notification.Presentation.Consumers;

public class PaymentEventConsumer : IConsumer<PaymentConfirmed>, IConsumer<PaymentFailed>
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly IEmailService _emailService;
    private readonly IUserLookupService _userLookup;
    private readonly ILogger<PaymentEventConsumer> _logger;

    public PaymentEventConsumer(IHubContext<NotificationHub> hub, IEmailService emailService, IUserLookupService userLookup, ILogger<PaymentEventConsumer> logger)
    {
        _hub = hub;
        _emailService = emailService;
        _userLookup = userLookup;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentConfirmed> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Payment {PaymentId} confirmed for order {OrderId}", msg.PaymentId, msg.OrderId);
        await _hub.Clients.Group($"user_{msg.UserId}").SendAsync("Notification", new
        {
            Type = "payment.confirmed",
            Title = "Payment Confirmed",
            Message = $"Payment of {msg.Amount:C} confirmed.",
            msg.OrderId
        });

        var user = await _userLookup.GetUserAsync(msg.UserId, context.CancellationToken);
        if (user != null)
        {
            await _emailService.SendEmailWithTemplateAsync(user.Email, "payment.confirmed", new Dictionary<string, string>
            {
                ["Amount"] = msg.Amount.ToString("C"),
                ["OrderNumber"] = msg.OrderId.ToString(),
                ["CustomerName"] = user.FullName
            }, context.CancellationToken);
        }
    }

    public async Task Consume(ConsumeContext<PaymentFailed> context)
    {
        var msg = context.Message;
        _logger.LogWarning("Payment failed for order {OrderId}: {FailureReason}", msg.OrderId, msg.FailureReason);
        await _hub.Clients.Group($"user_{msg.UserId}").SendAsync("Notification", new
        {
            Type = "payment.failed",
            Title = "Payment Failed",
            Message = $"Payment failed: {msg.FailureReason}",
            msg.OrderId
        });

        var user = await _userLookup.GetUserAsync(msg.UserId, context.CancellationToken);
        if (user != null)
        {
            await _emailService.SendEmailWithTemplateAsync(user.Email, "payment.failed", new Dictionary<string, string>
            {
                ["FailureReason"] = msg.FailureReason ?? "Unknown error",
                ["OrderNumber"] = msg.OrderId.ToString(),
                ["CustomerName"] = user.FullName
            }, context.CancellationToken);
        }
    }
}
