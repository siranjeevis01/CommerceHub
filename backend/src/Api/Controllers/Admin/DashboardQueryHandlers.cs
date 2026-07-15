using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Api.Application.Commands.Admin;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly CommerceHub.Modules.Identity.Infrastructure.Data.IdentityDbContext _identityDb;
    private readonly CommerceHub.Modules.Product.Infrastructure.Data.ProductDbContext _productDb;
    private readonly CommerceHub.Modules.Order.Infrastructure.Data.OrderDbContext _orderDb;
    private readonly CommerceHub.Modules.Vendor.Infrastructure.Data.VendorDbContext _vendorDb;

    public GetDashboardStatsQueryHandler(
        CommerceHub.Modules.Identity.Infrastructure.Data.IdentityDbContext identityDb,
        CommerceHub.Modules.Product.Infrastructure.Data.ProductDbContext productDb,
        CommerceHub.Modules.Order.Infrastructure.Data.OrderDbContext orderDb,
        CommerceHub.Modules.Vendor.Infrastructure.Data.VendorDbContext vendorDb)
    {
        _identityDb = identityDb;
        _productDb = productDb;
        _orderDb = orderDb;
        _vendorDb = vendorDb;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var totalUsers = await _identityDb.Users.CountAsync(cancellationToken);
        var totalVendors = await _vendorDb.Vendors.CountAsync(cancellationToken);
        var totalProducts = await _productDb.Products.CountAsync(cancellationToken);
        var totalOrders = await _orderDb.Orders.CountAsync(cancellationToken);
        var totalRevenue = await _orderDb.Orders.SumAsync(o => o.TotalAmount, cancellationToken);
        var pendingApprovals = await _vendorDb.Vendors.CountAsync(v => v.VerificationStatus == "Pending", cancellationToken);

        return new DashboardStatsDto
        {
            TotalUsers = totalUsers,
            TotalVendors = totalVendors,
            TotalProducts = totalProducts,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            PendingApprovals = pendingApprovals
        };
    }
}

public class GetRevenueChartQueryHandler : IRequestHandler<GetRevenueChartQuery, List<RevenueChartEntry>>
{
    private readonly CommerceHub.Modules.Order.Infrastructure.Data.OrderDbContext _orderDb;

    public GetRevenueChartQueryHandler(CommerceHub.Modules.Order.Infrastructure.Data.OrderDbContext orderDb)
    {
        _orderDb = orderDb;
    }

    public async Task<List<RevenueChartEntry>> Handle(GetRevenueChartQuery request, CancellationToken cancellationToken)
    {
        var since = DateTime.UtcNow.AddDays(-30);
        var orders = await _orderDb.Orders
            .Where(o => o.CreatedAt >= since)
            .ToListAsync(cancellationToken);

        var grouped = request.GroupBy?.ToLower() switch
        {
            "week" => orders.GroupBy(o => ISOWeek.GetWeekOfYear(o.CreatedAt).ToString()),
            "month" => orders.GroupBy(o => o.CreatedAt.ToString("yyyy-MM")),
            _ => orders.GroupBy(o => o.CreatedAt.ToString("yyyy-MM-dd"))
        };

        return grouped.Select(g => new RevenueChartEntry
        {
            Period = g.Key,
            Revenue = g.Sum(x => x.TotalAmount),
            OrderCount = g.Count()
        }).OrderBy(x => x.Period).ToList();
    }
}

public class GetTopProductsQueryHandler : IRequestHandler<GetTopProductsQuery, List<TopProductEntry>>
{
    private readonly CommerceHub.Modules.Order.Infrastructure.Data.OrderDbContext _orderDb;
    private readonly CommerceHub.Modules.Product.Infrastructure.Data.ProductDbContext _productDb;

    public GetTopProductsQueryHandler(
        CommerceHub.Modules.Order.Infrastructure.Data.OrderDbContext orderDb,
        CommerceHub.Modules.Product.Infrastructure.Data.ProductDbContext productDb)
    {
        _orderDb = orderDb;
        _productDb = productDb;
    }

    public async Task<List<TopProductEntry>> Handle(GetTopProductsQuery request, CancellationToken cancellationToken)
    {
        var result = await _orderDb.OrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new TopProductEntry
            {
                ProductId = g.Key,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(request.Top)
            .ToListAsync(cancellationToken);

        var productIds = result.Select(r => r.ProductId).ToList();
        var products = await _productDb.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.MainImageUrl, p.Price })
            .ToListAsync(cancellationToken);

        var productDict = products.ToDictionary(p => p.Id);
        return result.Select(r =>
        {
            var p = productDict.TryGetValue(r.ProductId, out var prod) ? prod : null;
            return new TopProductEntry
            {
                ProductId = r.ProductId,
                ProductName = p?.Name,
                MainImageUrl = p?.MainImageUrl,
                Price = p?.Price,
                TotalQuantity = r.TotalQuantity,
                TotalRevenue = r.TotalRevenue
            };
        }).ToList();
    }
}
