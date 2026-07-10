using Microsoft.Extensions.Logging;
using CommerceHub.Notification.Application.Common.Interfaces;

namespace CommerceHub.Notification.Infrastructure.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(ILogger<PushNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendPushNotificationAsync(int userId, string title, string message, string? imageUrl = null, string? linkUrl = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Push notification sent to user {UserId}: {Title} - {Message}", userId, title, message);
        return Task.CompletedTask;
    }
}
