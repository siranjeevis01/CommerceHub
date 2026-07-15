namespace CommerceHub.Modules.Notification.Application.Common.Interfaces;

public interface INotificationHub
{
    Task SendNotificationToUser(int userId, object notification);
    Task SendNotificationToAll(object notification);
}
