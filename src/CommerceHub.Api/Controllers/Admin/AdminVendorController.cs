using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Vendor.Infrastructure.Data;
using CommerceHub.Vendor.Domain.Entities;
using CommerceHub.Order.Infrastructure.Data;
using CommerceHub.Product.Infrastructure.Data;

namespace CommerceHub.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/vendors")]
[Authorize(Roles = "Admin")]
public class AdminVendorController : ControllerBase
{
    private readonly VendorDbContext _vendorDb;
    private readonly OrderDbContext _orderDb;
    private readonly ProductDbContext _productDb;

    public AdminVendorController(VendorDbContext vendorDb, OrderDbContext orderDb, ProductDbContext productDb)
    {
        _vendorDb = vendorDb;
        _orderDb = orderDb;
        _productDb = productDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetVendors(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _vendorDb.Vendors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(v => v.StoreName.ToLower().Contains(term) ||
                                     (v.BusinessEmail != null && v.BusinessEmail.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var vendors = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new
            {
                v.Id,
                v.UserId,
                v.StoreName,
                v.VerificationStatus,
                v.CommissionRate,
                v.TotalSales,
                v.TotalEarnings,
                v.Balance,
                v.CreatedAt
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, data = vendors });
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingVendors([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _vendorDb.Vendors.Where(v => v.VerificationStatus == "Pending");

        var totalCount = await query.CountAsync();
        var vendors = await query
            .OrderBy(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new
            {
                v.Id,
                v.UserId,
                v.StoreName,
                v.StoreDescription,
                v.BusinessEmail,
                v.BusinessPhone,
                v.GSTNumber,
                v.PANNumber,
                v.BusinessType,
                v.VerificationStatus,
                v.CommissionRate,
                v.CreatedAt
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, data = vendors });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVendor(int id)
    {
        var vendor = await _vendorDb.Vendors
            .Where(v => v.Id == id)
            .Select(v => new
            {
                v.Id,
                v.UserId,
                v.StoreName,
                v.StoreDescription,
                v.StoreLogo,
                v.StoreBanner,
                v.BusinessPhone,
                v.BusinessEmail,
                v.GSTNumber,
                v.PANNumber,
                v.BusinessType,
                v.BusinessAddress,
                v.VerificationStatus,
                v.CommissionRate,
                v.TotalSales,
                v.TotalEarnings,
                v.Balance,
                v.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (vendor == null) return NotFound();
        return Ok(vendor);
    }

    [HttpGet("{id}/products")]
    public async Task<IActionResult> GetVendorProducts(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var vendorExists = await _vendorDb.Vendors.AnyAsync(v => v.Id == id);
        if (!vendorExists) return NotFound();

        var totalCount = await _productDb.Products.CountAsync(p => p.VendorId == id);
        var products = await _productDb.Products
            .Where(p => p.VendorId == id)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.SKU,
                p.Price,
                p.StockQuantity,
                p.IsActive,
                p.MainImageUrl,
                p.CreatedAt
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, data = products });
    }

    [HttpGet("{id}/payouts")]
    public async Task<IActionResult> GetVendorPayouts(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var vendorExists = await _vendorDb.Vendors.AnyAsync(v => v.Id == id);
        if (!vendorExists) return NotFound();

        var totalCount = await _vendorDb.Payouts.CountAsync(p => p.VendorId == id);
        var payouts = await _vendorDb.Payouts
            .Where(p => p.VendorId == id)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.PayoutNumber,
                p.Amount,
                p.Status,
                p.PaymentMethod,
                p.TransactionId,
                p.CreatedAt
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, data = payouts });
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveVendor(int id)
    {
        var vendor = await _vendorDb.Vendors.FindAsync(id);
        if (vendor == null) return NotFound();

        vendor.VerificationStatus = "Active";
        vendor.UpdatedAt = DateTime.UtcNow;
        await _vendorDb.SaveChangesAsync();
        return Ok(new { message = "Vendor approved" });
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectVendor(int id)
    {
        var vendor = await _vendorDb.Vendors.FindAsync(id);
        if (vendor == null) return NotFound();

        vendor.VerificationStatus = "Rejected";
        vendor.UpdatedAt = DateTime.UtcNow;
        await _vendorDb.SaveChangesAsync();
        return Ok(new { message = "Vendor rejected" });
    }

    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> SuspendVendor(int id)
    {
        var vendor = await _vendorDb.Vendors.FindAsync(id);
        if (vendor == null) return NotFound();

        vendor.VerificationStatus = "Suspended";
        vendor.UpdatedAt = DateTime.UtcNow;
        await _vendorDb.SaveChangesAsync();
        return Ok(new { message = "Vendor suspended" });
    }
}
