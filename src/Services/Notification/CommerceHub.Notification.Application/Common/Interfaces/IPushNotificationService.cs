namespace CommerceHub.Notification.Application.Common.Interfaces;

public interface IPushNotificationService
{
    Task SendPushNotificationAsync(int userId, string title, string message, string? imageUrl = null, string? linkUrl = null, CancellationToken cancellationToken = default);
    Task SendBulkPushNotificationAsync(IEnumerable<int> userIds, string title, string message, string? imageUrl = null, string? linkUrl = null, CancellationToken cancellationToken = default);
}
