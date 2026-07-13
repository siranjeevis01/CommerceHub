using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Order.Infrastructure.Data;
using CommerceHub.Product.Infrastructure.Data;
using CommerceHub.Vendor.Infrastructure.Data;

namespace CommerceHub.Api.Controllers.Vendor;

[ApiController]
[Route("api/v1/vendor/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly VendorDbContext _vendorDb;
    private readonly ProductDbContext _productDb;
    private readonly OrderDbContext _orderDb;

    public DashboardController(
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

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var products = await _productDb.Products
            .Where(p => p.VendorId == vendorId && !p.IsDeleted)
            .ToListAsync();

        var orderItems = await _orderDb.OrderItems
            .Where(oi => oi.VendorId == vendorId)
            .Include(oi => oi.Order)
            .ToListAsync();

        var totalProducts = products.Count;
        var totalOrders = orderItems.Select(oi => oi.OrderId).Distinct().Count();
        var totalRevenue = orderItems
            .Where(oi => oi.Order.OrderStatus != "Cancelled" && oi.Order.OrderStatus != "Refunded")
            .Sum(oi => oi.TotalPrice);
        var pendingOrders = orderItems
            .Where(oi => oi.Order.OrderStatus == "Pending" || oi.Order.OrderStatus == "Processing")
            .Select(oi => oi.OrderId).Distinct().Count();

        var productIds = products.Select(p => p.Id).ToList();
        var reviews = await _productDb.Reviews
            .Where(r => productIds.Contains(r.ProductId))
            .ToListAsync();
        var averageRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0;

        var recentViews = await _productDb.Products
            .Where(p => p.VendorId == vendorId && !p.IsDeleted)
            .SumAsync(p => p.StockQuantity);

        return Ok(new
        {
            totalProducts,
            totalOrders,
            totalRevenue,
            pendingOrders,
            averageRating = Math.Round(averageRating, 2),
            recentViews
        });
    }

    [HttpGet("sales")]
    public async Task<IActionResult> GetSales([FromQuery] int days = 30)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var since = DateTime.UtcNow.AddDays(-days);

        var salesData = await _orderDb.OrderItems
            .Where(oi => oi.VendorId == vendorId &&
                         oi.Order.CreatedAt >= since &&
                         oi.Order.OrderStatus != "Cancelled")
            .GroupBy(oi => oi.Order.CreatedAt.Date)
            .Select(g => new
            {
                date = g.Key,
                orders = g.Select(x => x.OrderId).Distinct().Count(),
                revenue = g.Sum(x => x.TotalPrice),
                items = g.Sum(x => x.Quantity)
            })
            .OrderBy(x => x.date)
            .ToListAsync();

        return Ok(salesData);
    }

    [HttpGet("recent-orders")]
    public async Task<IActionResult> GetRecentOrders([FromQuery] int limit = 10)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var orders = await _orderDb.OrderItems
            .Where(oi => oi.VendorId == vendorId)
            .Include(oi => oi.Order)
            .OrderByDescending(oi => oi.Order.CreatedAt)
            .Take(limit)
            .Select(oi => new
            {
                orderId = oi.Order.Id,
                orderNumber = oi.Order.OrderNumber,
                status = oi.Order.OrderStatus,
                totalAmount = oi.TotalPrice,
                quantity = oi.Quantity,
                createdAt = oi.Order.CreatedAt
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var lowStockProducts = await _productDb.Products
            .Where(p => p.VendorId == vendorId && !p.IsDeleted && p.StockQuantity < 10)
            .Select(p => new
            {
                productId = p.Id,
                name = p.Name,
                sku = p.SKU,
                stockQuantity = p.StockQuantity,
                price = p.Price,
                mainImageUrl = p.MainImageUrl
            })
            .ToListAsync();

        return Ok(lowStockProducts);
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int limit = 10)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var topProducts = await _orderDb.OrderItems
            .Where(oi => oi.VendorId == vendorId && oi.Order.OrderStatus != "Cancelled")
            .GroupBy(oi => new { oi.ProductId, oi.UnitPrice })
            .Select(g => new
            {
                productId = g.Key.ProductId,
                totalSold = g.Sum(x => x.Quantity),
                totalRevenue = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.totalRevenue)
            .Take(limit)
            .ToListAsync();

        var productIds = topProducts.Select(x => x.productId).ToList();
        var products = await _productDb.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        var result = topProducts.Select(tp =>
        {
            var product = products.FirstOrDefault(p => p.Id == tp.productId);
            return new
            {
                tp.productId,
                name = product?.Name ?? "Unknown",
                sku = product?.SKU ?? "",
                mainImageUrl = product?.MainImageUrl,
                tp.totalSold,
                tp.totalRevenue
            };
        });

        return Ok(result);
    }

    [HttpGet("recent-reviews")]
    public async Task<IActionResult> GetRecentReviews([FromQuery] int limit = 10)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var productIds = await _productDb.Products
            .Where(p => p.VendorId == vendorId && !p.IsDeleted)
            .Select(p => p.Id)
            .ToListAsync();

        var reviews = await _productDb.Reviews
            .Where(r => productIds.Contains(r.ProductId))
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .Select(r => new
            {
                reviewId = r.Id,
                productId = r.ProductId,
                productName = r.Product.Name,
                rating = r.Rating,
                comment = r.Comment,
                userId = r.UserId,
                createdAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(reviews);
    }
}
