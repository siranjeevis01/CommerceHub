using CommerceHub.Modules.Notification.Domain.Models;

namespace CommerceHub.Modules.Notification.Application.Common.Interfaces;

public interface INotificationRepository
{
    Task<NotificationMessage?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationMessage>> GetByUserIdAsync(int userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(NotificationMessage notification, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(int id, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
}
