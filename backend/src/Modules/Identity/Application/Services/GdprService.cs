using System.IO.Compression;
using System.Text.Json;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CommerceHub.Modules.Identity.Application.Services;

public class GdprService : IGdprService
{
    private readonly IIdentityDbContext _context;
    private readonly ILogger<GdprService> _logger;

    public GdprService(IIdentityDbContext context, ILogger<GdprService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error, byte[]? Data)> ExportUserDataAsync(int userId)
    {
        var user = await _context.Users
            .FindAsync(userId);

        if (user == null)
            return (false, "User not found", null);

        var addresses = _context.Addresses
            .Where(a => a.UserId == userId)
            .ToList();

        var data = new
        {
            Profile = new
            {
                user.Email,
                user.Username,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.CreatedAt
            },
            Addresses = addresses.Select(a => new { a.AddressLine1, a.City, a.State, a.PostalCode, a.Country })
        };

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            var entry = zip.CreateEntry("userdata.json");
            using var writer = new StreamWriter(entry.Open());
            await writer.WriteAsync(json);
        }

        return (true, null, stream.ToArray());
    }

    public async Task<(bool Success, string? Error)> DeleteUserDataAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found");

        user.Email = $"deleted_{user.Id}@deleted.com";
        user.Username = $"deleted_{user.Id}";
        user.FirstName = "Deleted";
        user.LastName = "User";
        user.PhoneNumber = null;
        user.AvatarUrl = null;
        user.IsActive = false;
        user.IsDeleted = true;
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        user.TwoFactorSecret = null;
        user.TwoFactorEnabled = false;
        user.PasswordHash = "deleted";
        user.PasswordSalt = "deleted";

        await _context.SaveChangesAsync();
        _logger.LogInformation("User {UserId} data anonymized and deleted", userId);

        return (true, "Account deleted successfully");
    }
}
