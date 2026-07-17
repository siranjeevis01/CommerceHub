namespace CommerceHub.Modules.Identity.Application.Common.Interfaces;

public interface IGdprService
{
    Task<(bool Success, string? Error, byte[]? Data)> ExportUserDataAsync(int userId);
    Task<(bool Success, string? Error)> DeleteUserDataAsync(int userId);
}
