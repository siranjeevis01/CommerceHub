namespace CommerceHub.Notification.Application.Common.Interfaces;

public interface IUserLookupService
{
    Task<UserInfo?> GetUserAsync(int userId, CancellationToken ct = default);
    Task CacheUserAsync(int userId, string email, string firstName, string lastName, CancellationToken ct = default);
}

public class UserInfo
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
}
