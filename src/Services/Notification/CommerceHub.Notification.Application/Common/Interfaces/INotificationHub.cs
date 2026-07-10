namespace CommerceHub.Notification.Application.Common.Interfaces;

public interface INotificationHub
{
    Task SendNotificationToUser(int userId, object notification);
    Task SendNotificationToAll(object notification);
}
