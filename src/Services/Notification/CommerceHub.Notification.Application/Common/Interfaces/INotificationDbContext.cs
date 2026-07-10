using Microsoft.EntityFrameworkCore;
using CommerceHub.Notification.Domain.Models;

namespace CommerceHub.Notification.Application.Common.Interfaces;

public interface INotificationDbContext
{
    DbSet<NotificationMessage> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
