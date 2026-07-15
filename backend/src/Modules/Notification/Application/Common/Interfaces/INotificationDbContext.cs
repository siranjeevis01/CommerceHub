using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Notification.Domain.Models;

namespace CommerceHub.Modules.Notification.Application.Common.Interfaces;

public interface INotificationDbContext
{
    DbSet<NotificationMessage> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
