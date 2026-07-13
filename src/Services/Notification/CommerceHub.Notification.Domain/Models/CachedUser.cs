namespace CommerceHub.Notification.Domain.Models;

public class CachedUser
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}
