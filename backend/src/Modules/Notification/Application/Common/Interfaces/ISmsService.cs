namespace CommerceHub.Modules.Notification.Application.Common.Interfaces;

public interface ISmsService
{
    Task<bool> SendSmsAsync(string to, string message, CancellationToken cancellationToken = default);
}
