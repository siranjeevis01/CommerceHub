using Microsoft.AspNetCore.SignalR;
using CommerceHub.Modules.Notification.Application.Common.Interfaces;
using CommerceHub.Modules.Notification.Infrastructure.Hubs;

namespace CommerceHub.Modules.Notification.Infrastructure.Services;

public class NotificationService : INotificationHub
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationToUser(int userId, object notification)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
    }

    public async Task SendNotificationToAll(object notification)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
    }
}
