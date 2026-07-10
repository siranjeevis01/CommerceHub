using MassTransit;
using CommerceHub.Shared.Contracts.Events;
using CommerceHub.Notification.Application.Common.Interfaces;

namespace CommerceHub.Notification.Application.Consumers;

public class SendSmsNotificationConsumer : IConsumer<SendSmsNotification>
{
    private readonly ISmsService _smsService;

    public SendSmsNotificationConsumer(ISmsService smsService)
    {
        _smsService = smsService;
    }

    public async Task Consume(ConsumeContext<SendSmsNotification> context)
    {
        var message = context.Message;
        await _smsService.SendSmsAsync(message.To, message.Message, context.CancellationToken);
    }
}
