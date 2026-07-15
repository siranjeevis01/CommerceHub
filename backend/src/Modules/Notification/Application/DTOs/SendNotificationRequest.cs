namespace CommerceHub.Modules.Notification.Application.DTOs;

public class SendNotificationRequest
{
    public int? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? ImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string? Data { get; set; }
}
