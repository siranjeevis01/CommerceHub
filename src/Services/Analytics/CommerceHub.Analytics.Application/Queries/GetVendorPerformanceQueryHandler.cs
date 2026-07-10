using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Analytics.Application.Common.Interfaces;

namespace CommerceHub.Analytics.Application.Queries;

public class GetVendorPerformanceQueryHandler : IRequestHandler<GetVendorPerformanceQuery, VendorPerformanceDto>
{
    private readonly IAnalyticsDbContext _dbContext;

    public GetVendorPerformanceQueryHandler(IAnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VendorPerformanceDto> Handle(GetVendorPerformanceQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.AnalyticsEvents
            .Where(e => e.EventType == "OrderPlaced");

        if (request.From.HasValue)
            query = query.Where(e => e.Timestamp >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(e => e.Timestamp <= request.To.Value);

        var orderEvents = await query
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync(cancellationToken);

        var vendorItems = orderEvents
            .Select(e =>
            {
                try
                {
                    var data = JsonSerializer.Deserialize<OrderEventData>(e.EventData ?? "{}");
                    return new { Event = e, Data = data };
                }
                catch
                {
                    return null;
                }
            })
            .Where(x => x?.Data?.Items is not null)
            .SelectMany(x => x!.Data!.Items!
                .Where(i => i.VendorId == request.VendorId)
                .Select(i => new { x.Event, Item = i }))
            .ToList();

        var totalRevenue = vendorItems.Sum(x => x.Item.Quantity * x.Item.UnitPrice);
        var totalProductsSold = vendorItems.Sum(x => x.Item.Quantity);
        var orderIds = vendorItems.Select(x => x.Event.Id).Distinct().Count();

        var commissionEvents = await _dbContext.AnalyticsEvents
            .CountAsync(e => e.EventType == "CommissionCalculated"
                             && e.Timestamp >= (request.From ?? DateTime.MinValue)
                             && e.Timestamp <= (request.To ?? DateTime.MaxValue), cancellationToken);

        var totalCommission = commissionEvents * 0.1m * totalRevenue;

        var timestamps = vendorItems.Select(x => x.Event.Timestamp).ToList();

        return new VendorPerformanceDto
        {
            VendorId = request.VendorId,
            TotalOrders = orderIds,
            TotalProductsSold = totalProductsSold,
            TotalRevenue = totalRevenue,
            TotalCommission = totalCommission,
            NetEarnings = totalRevenue - totalCommission,
            AvgOrderValue = orderIds > 0 ? totalRevenue / orderIds : 0,
            FirstSale = timestamps.MinBy(t => t),
            LastSale = timestamps.MaxBy(t => t)
        };
    }

    private class OrderEventData
    {
        public List<OrderItemData>? Items { get; set; }
    }

    private class OrderItemData
    {
        public int ProductId { get; set; }
        public int VendorId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
