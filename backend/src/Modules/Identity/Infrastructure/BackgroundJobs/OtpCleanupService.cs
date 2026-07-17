using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;

namespace CommerceHub.Modules.Identity.Infrastructure.BackgroundJobs;

public class OtpCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OtpCleanupService> _logger;

    public OtpCleanupService(IServiceProvider serviceProvider, ILogger<OtpCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Cleaning expired OTPs...");
            using var scope = _serviceProvider.CreateScope();
            var identityDbContext = scope.ServiceProvider.GetRequiredService<IIdentityDbContext>();

            try
            {
                var cutoff = DateTime.UtcNow.AddHours(-2);
                var expired = await identityDbContext.Otps
                    .Where(o => o.CreatedAt < cutoff || o.IsUsed)
                    .ToListAsync(stoppingToken);

                if (expired.Any())
                {
                    identityDbContext.Otps.RemoveRange(expired);
                    await identityDbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Removed {Count} expired OTPs", expired.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean expired OTPs");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
