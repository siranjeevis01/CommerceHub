using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Analytics.Application.Common.Interfaces;
using CommerceHub.Analytics.Application.DTOs;

namespace CommerceHub.Analytics.Application.Queries;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IAnalyticsDbContext _dbContext;

    public GetDashboardSummaryQueryHandler(IAnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var todayEnd = today.AddDays(1);

        var latestSummary = await _dbContext.DailySummaries
            .OrderByDescending(d => d.Date)
            .FirstOrDefaultAsync(cancellationToken);

        var todayEvents = await _dbContext.AnalyticsEvents
            .Where(e => e.Timestamp >= today && e.Timestamp < todayEnd)
            .ToListAsync(cancellationToken);

        var todayOrders = todayEvents.Count(e => e.EventType is "OrderPlaced" or "OrderConfirmed");
        var todayRevenue = todayEvents
            .Where(e => e.EventType == "PaymentConfirmed")
            .Select(e => decimal.TryParse(e.EventData, out var amt) ? amt : 0)
            .Sum();

        var totalUsers = await _dbContext.AnalyticsEvents
            .Where(e => e.EventType == "UserRegistered")
            .Select(e => e.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var todayNewUsers = todayEvents
            .Where(e => e.EventType == "UserRegistered")
            .Select(e => e.UserId)
            .Distinct()
            .Count();

        var totalOrders = await _dbContext.AnalyticsEvents
            .CountAsync(e => e.EventType == "OrderPlaced", cancellationToken);

        var paymentEvents = await _dbContext.AnalyticsEvents
            .Where(e => e.EventType == "PaymentConfirmed")
            .ToListAsync(cancellationToken);
        var totalRevenue = paymentEvents
            .Select(e => decimal.TryParse(e.EventData, out var amt) ? amt : 0)
            .Sum();

        var totalVendors = await _dbContext.AnalyticsEvents
            .Where(e => e.EventType == "VendorSettled")
            .Select(e => e.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var conversionRate = totalUsers > 0
            ? (double)totalOrders / totalUsers * 100
            : 0;

        var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        return new DashboardSummaryDto
        {
            TotalUsers = totalUsers,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            TotalVendors = totalVendors,
            AvgOrderValue = avgOrderValue,
            ConversionRate = Math.Round(conversionRate, 2),
            NewUsersToday = todayNewUsers,
            OrdersToday = todayOrders,
            RevenueToday = todayRevenue
        };
    }
}
