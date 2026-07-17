using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CommerceHub.Modules.Notification.Application.Common.Interfaces;

namespace CommerceHub.Modules.Notification.Infrastructure.BackgroundJobs;

public class EmailQueueService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailQueueService> _logger;

    public EmailQueueService(IServiceProvider serviceProvider, ILogger<EmailQueueService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Processing email queue...");
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            try
            {
                _logger.LogInformation("Email queue check completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process email queue");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
