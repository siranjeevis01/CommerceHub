namespace CommerceHub.Notification.Application.Common.Interfaces;

public interface IWhatsAppService
{
    Task<bool> SendWhatsAppMessageAsync(string to, string message, CancellationToken cancellationToken = default);
}
