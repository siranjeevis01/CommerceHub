using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Analytics.Application.Common.Interfaces;
using CommerceHub.Analytics.Application.DTOs;

namespace CommerceHub.Analytics.Application.Queries;

public class GetTopProductsQueryHandler : IRequestHandler<GetTopProductsQuery, List<TopProductDto>>
{
    private readonly IAnalyticsDbContext _dbContext;

    public GetTopProductsQueryHandler(IAnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TopProductDto>> Handle(GetTopProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.AnalyticsEvents
            .Where(e => e.EventType == "OrderPlaced");

        if (request.From.HasValue)
            query = query.Where(e => e.Timestamp >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(e => e.Timestamp <= request.To.Value);

        var orderEvents = await query.ToListAsync(cancellationToken);

        var productSales = orderEvents
            .Select(e =>
            {
                try
                {
                    return JsonSerializer.Deserialize<OrderEventData>(e.EventData ?? "{}");
                }
                catch
                {
                    return null;
                }
            })
            .Where(d => d?.Items is not null)
            .SelectMany(d => d!.Items!)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName ?? "Unknown",
                TotalSold = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.Quantity * i.UnitPrice)
            })
            .OrderByDescending(p => p.TotalSold)
            .Take(request.Count)
            .ToList();

        return productSales;
    }

    private class OrderEventData
    {
        public List<OrderItemData>? Items { get; set; }
    }

    private class OrderItemData
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
