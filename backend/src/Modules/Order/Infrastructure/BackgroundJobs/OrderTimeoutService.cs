using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Order.Application.Common.Interfaces;

namespace CommerceHub.Modules.Order.Infrastructure.BackgroundJobs;

public class OrderTimeoutService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderTimeoutService> _logger;

    public OrderTimeoutService(IServiceProvider serviceProvider, ILogger<OrderTimeoutService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Checking for pending order timeouts...");
            using var scope = _serviceProvider.CreateScope();
            var orderDbContext = scope.ServiceProvider.GetRequiredService<IOrderDbContext>();

            try
            {
                var cutoff = DateTime.UtcNow.AddMinutes(-30);
                var pendingOrders = await orderDbContext.Orders
                    .Where(o => o.OrderStatus == "Pending" && o.CreatedAt < cutoff)
                    .ToListAsync(stoppingToken);

                foreach (var order in pendingOrders)
                {
                    order.OrderStatus = "Cancelled";
                    order.UpdatedAt = DateTime.UtcNow;
                    order.StatusHistories.Add(new Domain.Entities.OrderStatusHistory
                    {
                        OrderId = order.Id,
                        FromStatus = "Pending",
                        ToStatus = "Cancelled",
                        Remarks = "Auto-cancelled due to timeout"
                    });
                }

                if (pendingOrders.Any())
                {
                    await orderDbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Auto-cancelled {Count} pending orders", pendingOrders.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check order timeouts");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
