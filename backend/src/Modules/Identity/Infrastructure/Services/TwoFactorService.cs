using System.Text;
using OtpNet;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;
using CommerceHub.Modules.Identity.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace CommerceHub.Modules.Identity.Infrastructure.Services;

public class TwoFactorService : ITwoFactorService
{
    private readonly IIdentityDbContext _context;
    private readonly ILogger<TwoFactorService> _logger;

    public TwoFactorService(IIdentityDbContext context, ILogger<TwoFactorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error, TwoFactorSetupDto? Data)> EnableTwoFactorAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found", null);

        var key = KeyGeneration.GenerateRandomKey(20);
        var secret = Base32Encoding.ToString(key);
        var issuer = "CommerceHub";
        var email = user.Email;

        var otpAuthUri = $"otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}";

        user.TwoFactorSecret = secret;
        user.TwoFactorEnabled = false;
        await _context.SaveChangesAsync();

        return (true, null, new TwoFactorSetupDto
        {
            Secret = secret,
            QrCodeUri = otpAuthUri
        });
    }

    public async Task<(bool Success, string? Error)> VerifyTwoFactorAsync(int userId, string code)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
            return (false, "2FA not enabled or invalid");

        var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
        if (totp.VerifyTotp(code, out _))
        {
            user.TwoFactorEnabled = true;
            await _context.SaveChangesAsync();
            return (true, "2FA enabled successfully");
        }
        return (false, "Invalid verification code");
    }

    public async Task<(bool Success, string? Error)> DisableTwoFactorAsync(int userId, string code)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.TwoFactorEnabled)
            return (false, "2FA is not enabled");

        var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret!));
        if (!totp.VerifyTotp(code, out _))
            return (false, "Invalid code");

        user.TwoFactorSecret = null;
        user.TwoFactorEnabled = false;
        await _context.SaveChangesAsync();
        return (true, "2FA disabled");
    }

    public async Task<(bool Success, string? Error)> ValidateTwoFactorCodeAsync(int userId, string code)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
            return (false, "2FA not set up");

        var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
        if (totp.VerifyTotp(code, out _))
            return (true, null);

        return (false, "Invalid 2FA code");
    }
}
