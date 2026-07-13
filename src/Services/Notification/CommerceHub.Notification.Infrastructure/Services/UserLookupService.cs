using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CommerceHub.Notification.Application.Common.Interfaces;
using CommerceHub.Notification.Infrastructure.Persistence;

namespace CommerceHub.Notification.Infrastructure.Services;

public class UserLookupService : IUserLookupService
{
    private readonly NotificationDbContext _db;
    private readonly ILogger<UserLookupService> _logger;

    public UserLookupService(NotificationDbContext db, ILogger<UserLookupService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<UserInfo?> GetUserAsync(int userId, CancellationToken ct = default)
    {
        var cached = await _db.CachedUsers.FindAsync([userId], ct);
        if (cached == null)
        {
            _logger.LogWarning("User {UserId} not found in cache - email will not be sent", userId);
            return null;
        }

        return new UserInfo
        {
            UserId = cached.UserId,
            Email = cached.Email,
            FirstName = cached.FirstName,
            LastName = cached.LastName
        };
    }

    public async Task CacheUserAsync(int userId, string email, string firstName, string lastName, CancellationToken ct = default)
    {
        var existing = await _db.CachedUsers.FindAsync([userId], ct);
        if (existing != null)
        {
            existing.Email = email;
            existing.FirstName = firstName;
            existing.LastName = lastName;
            existing.CachedAt = DateTime.UtcNow;
        }
        else
        {
            _db.CachedUsers.Add(new Domain.Models.CachedUser
            {
                UserId = userId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CachedAt = DateTime.UtcNow
            });
        }
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Cached user {UserId} ({Email})", userId, email);
    }
}
