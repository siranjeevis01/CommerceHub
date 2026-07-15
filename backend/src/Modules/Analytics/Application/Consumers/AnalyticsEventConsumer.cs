using System.Text.Json;
using MassTransit;
using CommerceHub.Shared.Contracts.Events;
using CommerceHub.Modules.Analytics.Application.Common.Interfaces;
using CommerceHub.Modules.Analytics.Domain.Entities;

namespace CommerceHub.Modules.Analytics.Application.Consumers;

public class AnalyticsEventConsumer :
    IConsumer<OrderPlaced>,
    IConsumer<OrderConfirmed>,
    IConsumer<OrderCancelled>,
    IConsumer<PaymentConfirmed>,
    IConsumer<PaymentFailed>,
    IConsumer<ProductViewed>,
    IConsumer<UserRegistered>
{
    private readonly IAnalyticsDbContext _dbContext;

    public AnalyticsEventConsumer(IAnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var message = context.Message;
        await TrackEvent(
            "OrderPlaced",
            JsonSerializer.Serialize(new { message.OrderId, message.OrderNumber, message.TotalAmount, message.Items }),
            message.UserId,
            context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        var message = context.Message;
        await TrackEvent(
            "OrderConfirmed",
            JsonSerializer.Serialize(new { message.OrderId, message.OrderNumber }),
            message.UserId,
            context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<OrderCancelled> context)
    {
        var message = context.Message;
        await TrackEvent(
            "OrderCancelled",
            JsonSerializer.Serialize(new { message.OrderId, message.OrderNumber, message.Reason }),
            message.UserId,
            context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<PaymentConfirmed> context)
    {
        var message = context.Message;
        await TrackEvent(
            "PaymentConfirmed",
            JsonSerializer.Serialize(new { message.PaymentId, message.OrderId, message.Amount, message.TransactionId }),
            message.UserId,
            context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<PaymentFailed> context)
    {
        var message = context.Message;
        await TrackEvent(
            "PaymentFailed",
            JsonSerializer.Serialize(new { message.PaymentId, message.OrderId, message.FailureReason }),
            message.UserId,
            context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<ProductViewed> context)
    {
        var message = context.Message;
        await TrackEvent(
            "ProductViewed",
            JsonSerializer.Serialize(new { message.ProductId }),
            message.UserId,
            context.CancellationToken);

        var pageView = new AnalyticsEvent
        {
            EventType = "PageView",
            EventData = JsonSerializer.Serialize(new { message.ProductId }),
            UserId = message.UserId,
            SessionId = message.SessionId,
            Timestamp = message.ViewedAt
        };

        _dbContext.AnalyticsEvents.Add(pageView);
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        var message = context.Message;
        await TrackEvent(
            "UserRegistered",
            JsonSerializer.Serialize(new { message.UserId, message.Email, message.FirstName, message.LastName, message.Role }),
            message.UserId,
            context.CancellationToken);
    }

    private async Task TrackEvent(string eventType, string eventData, int? userId, CancellationToken cancellationToken)
    {
        var analyticsEvent = new AnalyticsEvent
        {
            EventType = eventType,
            EventData = eventData,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };

        _dbContext.AnalyticsEvents.Add(analyticsEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
