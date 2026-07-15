namespace CommerceHub.Modules.Notification.Application.Common.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task<bool> SendEmailWithTemplateAsync(string to, string templateName, Dictionary<string, string> templateData, CancellationToken cancellationToken = default);
}
