using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Identity.Infrastructure.Data;
using CommerceHub.Product.Infrastructure.Data;
using CommerceHub.Order.Infrastructure.Data;
using CommerceHub.Vendor.Infrastructure.Data;

namespace CommerceHub.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IdentityDbContext _identityDb;
    private readonly ProductDbContext _productDb;
    private readonly OrderDbContext _orderDb;
    private readonly VendorDbContext _vendorDb;

    public AdminDashboardController(
        IdentityDbContext identityDb,
        ProductDbContext productDb,
        OrderDbContext orderDb,
        VendorDbContext vendorDb)
    {
        _identityDb = identityDb;
        _productDb = productDb;
        _orderDb = orderDb;
        _vendorDb = vendorDb;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalUsers = await _identityDb.Users.CountAsync();
        var totalVendors = await _vendorDb.Vendors.CountAsync();
        var totalProducts = await _productDb.Products.CountAsync();
        var totalOrders = await _orderDb.Orders.CountAsync();
        var totalRevenue = await _orderDb.Orders.SumAsync(o => o.TotalAmount);
        var pendingApprovals = await _vendorDb.Vendors.CountAsync(v => v.VerificationStatus == "Pending");

        return Ok(new
        {
            totalUsers,
            totalVendors,
            totalProducts,
            totalOrders,
            totalRevenue,
            pendingApprovals
        });
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] string? groupBy = null)
    {
        var since = DateTime.UtcNow.AddDays(-30);

        var orders = await _orderDb.Orders
            .Where(o => o.CreatedAt >= since)
            .ToListAsync();

        var grouped = groupBy?.ToLower() switch
        {
            "week" => orders.GroupBy(o => ISOWeek.GetWeekOfYear(o.CreatedAt).ToString()),
            "month" => orders.GroupBy(o => o.CreatedAt.ToString("yyyy-MM")),
            _ => orders.GroupBy(o => o.CreatedAt.ToString("yyyy-MM-dd"))
        };

        var result = grouped.Select(g => new
        {
            period = g.Key,
            revenue = g.Sum(x => x.TotalAmount),
            orderCount = g.Count()
        }).OrderBy(x => x.period).ToList();

        return Ok(result);
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int top = 10)
    {
        var result = await _orderDb.OrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                productId = g.Key,
                totalQuantity = g.Sum(x => x.Quantity),
                totalRevenue = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.totalRevenue)
            .Take(top)
            .ToListAsync();

        var productIds = result.Select(r => r.productId).ToList();
        var products = await _productDb.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.MainImageUrl, p.Price })
            .ToListAsync();

        var productDict = products.ToDictionary(p => p.Id);
        var enriched = result.Select(r =>
        {
            var p = productDict.TryGetValue(r.productId, out var prod) ? prod : null;
            return new
            {
                r.productId,
                productName = p?.Name,
                mainImageUrl = p?.MainImageUrl,
                price = p?.Price,
                r.totalQuantity,
                r.totalRevenue
            };
        });

        return Ok(enriched);
    }
}
