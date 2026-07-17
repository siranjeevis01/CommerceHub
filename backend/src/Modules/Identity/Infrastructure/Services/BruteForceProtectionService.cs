using CommerceHub.Infrastructure.Cache;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CommerceHub.Modules.Identity.Infrastructure.Services;

public class BruteForceProtectionService : IBruteForceProtectionService
{
    private readonly ICacheService _cache;
    private readonly ILogger<BruteForceProtectionService> _logger;
    private const int MaxAttempts = 5;
    private static readonly TimeSpan BlockDuration = TimeSpan.FromMinutes(15);

    public BruteForceProtectionService(ICacheService cache, ILogger<BruteForceProtectionService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsBlockedAsync(string key)
    {
        var blockKey = $"blocked_{key}";
        return await _cache.ExistsAsync(blockKey);
    }

    public async Task RecordFailureAsync(string key)
    {
        var attemptKey = $"attempts_{key}";
        var attempts = await _cache.GetAsync<int?>(attemptKey) ?? 0;
        attempts++;

        if (attempts >= MaxAttempts)
        {
            var blockKey = $"blocked_{key}";
            await _cache.SetAsync(blockKey, true, BlockDuration);
            await _cache.RemoveAsync(attemptKey);
            _logger.LogWarning("IP/User {Key} blocked for {Minutes} minutes due to excessive failures.", key, BlockDuration.TotalMinutes);
        }
        else
        {
            await _cache.SetAsync(attemptKey, attempts, TimeSpan.FromHours(1));
        }
    }

    public async Task ResetAttemptsAsync(string key)
    {
        var attemptKey = $"attempts_{key}";
        await _cache.RemoveAsync(attemptKey);
        var blockKey = $"blocked_{key}";
        await _cache.RemoveAsync(blockKey);
    }
}
