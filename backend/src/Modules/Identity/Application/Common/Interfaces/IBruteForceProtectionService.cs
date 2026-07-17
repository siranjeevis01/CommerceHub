namespace CommerceHub.Modules.Identity.Application.Common.Interfaces;

public interface IBruteForceProtectionService
{
    Task<bool> IsBlockedAsync(string key);
    Task RecordFailureAsync(string key);
    Task ResetAttemptsAsync(string key);
}
