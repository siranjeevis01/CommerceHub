using MassTransit;
using CommerceHub.Shared.Contracts.Events;
using CommerceHub.Notification.Application.Common.Interfaces;

namespace CommerceHub.Notification.Application.Consumers;

public class SendPushNotificationConsumer : IConsumer<SendPushNotification>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationHub _notificationHub;

    public SendPushNotificationConsumer(
        IPushNotificationService pushNotificationService,
        INotificationHub notificationHub)
    {
        _pushNotificationService = pushNotificationService;
        _notificationHub = notificationHub;
    }

    public async Task Consume(ConsumeContext<SendPushNotification> context)
    {
        var message = context.Message;
        await _pushNotificationService.SendPushNotificationAsync(
            message.UserId,
            message.Title,
            message.Message,
            message.ImageUrl,
            message.LinkUrl,
            context.CancellationToken);

        await _notificationHub.SendNotificationToUser(message.UserId, new
        {
            UserId = message.UserId,
            Title = message.Title,
            Message = message.Message,
            ImageUrl = message.ImageUrl,
            LinkUrl = message.LinkUrl,
            CreatedAt = DateTime.UtcNow
        });
    }
}
