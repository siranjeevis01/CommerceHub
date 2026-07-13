using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Order.Infrastructure.Data;
using CommerceHub.Vendor.Infrastructure.Data;

namespace CommerceHub.Api.Controllers.Vendor;

[ApiController]
[Route("api/v1/vendor/commissions")]
[Authorize]
public class CommissionController : ControllerBase
{
    private readonly VendorDbContext _vendorDb;
    private readonly OrderDbContext _orderDb;

    public CommissionController(VendorDbContext vendorDb, OrderDbContext orderDb)
    {
        _vendorDb = vendorDb;
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
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var query = _orderDb.OrderItems
            .Where(oi => oi.VendorId == vendorId && oi.Commission > 0)
            .Include(oi => oi.Order)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(oi => oi.Order.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(oi => oi.Order.CreatedAt <= to.Value);

        var totalItems = await query.CountAsync();
        var commissions = await query
            .OrderByDescending(oi => oi.Order.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(oi => new
            {
                orderId = oi.Order.Id,
                orderNumber = oi.Order.OrderNumber,
                productId = oi.ProductId,
                unitPrice = oi.UnitPrice,
                quantity = oi.Quantity,
                totalPrice = oi.TotalPrice,
                commission = oi.Commission,
                vendorEarning = oi.VendorEarning,
                createdAt = oi.Order.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            items = commissions,
            totalItems,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        });
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var vendor = await _vendorDb.Vendors
            .FirstAsync(v => v.Id == vendorId);

        var query = _orderDb.OrderItems
            .Where(oi => oi.VendorId == vendorId && oi.Order.OrderStatus != "Cancelled")
            .Include(oi => oi.Order)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(oi => oi.Order.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(oi => oi.Order.CreatedAt <= to.Value);

        var items = await query.ToListAsync();

        var totalSales = items.Sum(i => i.TotalPrice);
        var totalCommission = items.Sum(i => i.Commission);
        var totalEarnings = items.Sum(i => i.VendorEarning);
        var totalOrders = items.Select(i => i.OrderId).Distinct().Count();
        var totalItemsSold = items.Sum(i => i.Quantity);

        return Ok(new
        {
            commissionRate = vendor.CommissionRate,
            totalSales,
            totalCommission,
            totalEarnings,
            totalOrders,
            totalItemsSold,
            averageOrderValue = totalOrders > 0 ? Math.Round(totalSales / totalOrders, 2) : 0
        });
    }
}
