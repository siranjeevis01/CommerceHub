using MassTransit;
using CommerceHub.Shared.Contracts.Events;
using CommerceHub.Notification.Application.Common.Interfaces;

namespace CommerceHub.Notification.Application.Consumers;

public class SendEmailNotificationConsumer : IConsumer<SendEmailNotification>
{
    private readonly IEmailService _emailService;

    public SendEmailNotificationConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<SendEmailNotification> context)
    {
        var message = context.Message;
        if (!string.IsNullOrEmpty(message.TemplateName) && message.TemplateData != null)
        {
            await _emailService.SendEmailWithTemplateAsync(
                message.To,
                message.TemplateName,
                message.TemplateData,
                context.CancellationToken);
        }
        else
        {
            await _emailService.SendEmailAsync(
                message.To,
                message.Subject,
                message.Body,
                message.IsHtml,
                context.CancellationToken);
        }
    }
}
