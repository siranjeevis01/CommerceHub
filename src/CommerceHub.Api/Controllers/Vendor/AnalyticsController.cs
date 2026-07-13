using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Order.Infrastructure.Data;
using CommerceHub.Product.Infrastructure.Data;
using CommerceHub.Vendor.Infrastructure.Data;

namespace CommerceHub.Api.Controllers.Vendor;

[ApiController]
[Route("api/v1/vendor/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly VendorDbContext _vendorDb;
    private readonly ProductDbContext _productDb;
    private readonly OrderDbContext _orderDb;

    public AnalyticsController(
        VendorDbContext vendorDb,
        ProductDbContext productDb,
        OrderDbContext orderDb)
    {
        _vendorDb = vendorDb;
        _productDb = productDb;
        _orderDb = orderDb;
    }

    private int GetUserId() => int.Parse(User.FindFirst("userId")!.Value);

    private async Task<int?> GetVendorIdAsync()
    {
        var userId = GetUserId();
        var vendor = await _vendorDb.Vendors
            .FirstOrDefaultAsync(v => v.UserId == userId && !v.IsDeleted);
        return vendor?.Id;
    }

    [HttpGet]
    public async Task<IActionResult> GetAnalytics(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var since = from ?? DateTime.UtcNow.AddDays(-30);
        var until = to ?? DateTime.UtcNow;

        var products = await _productDb.Products
            .Where(p => p.VendorId == vendorId && !p.IsDeleted)
            .ToListAsync();

        var orderItems = await _orderDb.OrderItems
            .Where(oi => oi.VendorId == vendorId)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.CreatedAt >= since && oi.Order.CreatedAt <= until)
            .ToListAsync();

        var activeOrders = orderItems.Where(oi => oi.Order.OrderStatus != "Cancelled").ToList();
        var cancelledOrders = orderItems.Where(oi => oi.Order.OrderStatus == "Cancelled").ToList();

        var totalRevenue = activeOrders.Sum(oi => oi.TotalPrice);
        var totalCommission = activeOrders.Sum(oi => oi.Commission);
        var totalEarnings = activeOrders.Sum(oi => oi.VendorEarning);
        var totalOrders = activeOrders.Select(oi => oi.OrderId).Distinct().Count();
        var totalItemsSold = activeOrders.Sum(oi => oi.Quantity);

        var categoryBreakdown = products
            .GroupBy(p => p.CategoryId)
            .Select(g => new
            {
                categoryId = g.Key,
                productCount = g.Count(),
                totalValue = g.Sum(p => p.Price)
            })
            .OrderByDescending(x => x.productCount)
            .ToList();

        var dailySales = activeOrders
            .GroupBy(oi => oi.Order.CreatedAt.Date)
            .Select(g => new
            {
                date = g.Key,
                revenue = g.Sum(x => x.TotalPrice),
                orders = g.Select(x => x.OrderId).Distinct().Count(),
                items = g.Sum(x => x.Quantity)
            })
            .OrderBy(x => x.date)
            .ToList();

        var topProducts = activeOrders
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                productId = g.Key,
                totalSold = g.Sum(x => x.Quantity),
                totalRevenue = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.totalRevenue)
            .Take(10)
            .ToList();

        var productIds = topProducts.Select(tp => tp.productId).ToList();
        var productNames = await _productDb.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.SKU })
            .ToListAsync();

        return Ok(new
        {
            period = new { from = since, to = until },
            summary = new
            {
                totalProducts = products.Count,
                activeProducts = products.Count(p => p.IsActive && p.IsPublished),
                totalRevenue,
                totalCommission,
                totalEarnings,
                totalOrders,
                totalItemsSold,
                cancelledOrders = cancelledOrders.Select(oi => oi.OrderId).Distinct().Count(),
                averageOrderValue = totalOrders > 0 ? Math.Round(totalRevenue / totalOrders, 2) : 0,
                averageItemPrice = totalItemsSold > 0 ? Math.Round(totalRevenue / totalItemsSold, 2) : 0
            },
            categoryBreakdown,
            dailySales,
            topProducts = topProducts.Select(tp =>
            {
                var product = productNames.FirstOrDefault(p => p.Id == tp.productId);
                return new
                {
                    tp.productId,
                    name = product?.Name ?? "Unknown",
                    sku = product?.SKU ?? "",
                    tp.totalSold,
                    tp.totalRevenue
                };
            })
        });
    }
}
