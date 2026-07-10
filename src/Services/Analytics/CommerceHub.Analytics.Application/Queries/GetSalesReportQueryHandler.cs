using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Analytics.Application.Common.Interfaces;
using CommerceHub.Analytics.Application.DTOs;

namespace CommerceHub.Analytics.Application.Queries;

public class GetSalesReportQueryHandler : IRequestHandler<GetSalesReportQuery, SalesReportDto>
{
    private readonly IAnalyticsDbContext _dbContext;

    public GetSalesReportQueryHandler(IAnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken cancellationToken)
    {
        var paymentEvents = await _dbContext.AnalyticsEvents
            .Where(e => e.EventType == "PaymentConfirmed"
                        && e.Timestamp >= request.From
                        && e.Timestamp <= request.To)
            .ToListAsync(cancellationToken);

        var orderEvents = await _dbContext.AnalyticsEvents
            .Where(e => e.EventType == "OrderPlaced"
                        && e.Timestamp >= request.From
                        && e.Timestamp <= request.To)
            .ToListAsync(cancellationToken);

        var dailySales = paymentEvents
            .GroupBy(e => e.Timestamp.Date)
            .Select(g => new DailySalesDto
            {
                Date = g.Key,
                OrderCount = orderEvents.Count(o => o.Timestamp.Date == g.Key),
                Revenue = g.Sum(e => decimal.TryParse(e.EventData, out var amt) ? amt : 0)
            })
            .OrderBy(d => d.Date)
            .ToList();

        var totalSales = dailySales.Sum(d => d.Revenue);
        var totalOrders = dailySales.Sum(d => d.OrderCount);
        var avgOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

        return new SalesReportDto
        {
            From = request.From,
            To = request.To,
            TotalSales = totalSales,
            TotalOrders = totalOrders,
            AvgOrderValue = avgOrderValue,
            DailySales = dailySales
        };
    }
}
