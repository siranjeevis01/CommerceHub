using CommerceHub.Modules.Identity.Application.DTOs;

namespace CommerceHub.Modules.Identity.Application.Common.Interfaces;

public interface ITwoFactorService
{
    Task<(bool Success, string? Error, TwoFactorSetupDto? Data)> EnableTwoFactorAsync(int userId);
    Task<(bool Success, string? Error)> VerifyTwoFactorAsync(int userId, string code);
    Task<(bool Success, string? Error)> DisableTwoFactorAsync(int userId, string code);
    Task<(bool Success, string? Error)> ValidateTwoFactorCodeAsync(int userId, string code);
}
