using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Vendor.Domain.Entities;
using CommerceHub.Vendor.Infrastructure.Data;

namespace CommerceHub.Api.Controllers.Vendor;

[ApiController]
[Route("api/v1/vendor/payouts")]
[Authorize]
public class PayoutController : ControllerBase
{
    private readonly VendorDbContext _vendorDb;

    public PayoutController(VendorDbContext vendorDb)
    {
        _vendorDb = vendorDb;
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
        [FromQuery] string? status = null)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var query = _vendorDb.Payouts
            .Where(p => p.VendorId == vendorId && !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);

        var totalItems = await query.CountAsync();
        var payouts = await query
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

        return Ok(new
        {
            items = payouts,
            totalItems,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        });
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var vendor = await _vendorDb.Vendors
            .FirstAsync(v => v.Id == vendorId);

        var payouts = await _vendorDb.Payouts
            .Where(p => p.VendorId == vendorId && !p.IsDeleted)
            .ToListAsync();

        return Ok(new
        {
            totalEarned = vendor.TotalEarnings,
            totalPaid = payouts.Where(p => p.Status == "Completed").Sum(p => p.Amount),
            pending = payouts.Where(p => p.Status == "Pending" || p.Status == "Processing").Sum(p => p.Amount),
            balance = vendor.Balance,
            payoutCount = payouts.Count,
            completedPayoutCount = payouts.Count(p => p.Status == "Completed")
        });
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestPayout([FromBody] RequestPayoutRequest request)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var vendor = await _vendorDb.Vendors
            .FirstAsync(v => v.Id == vendorId);

        if (vendor.Balance <= 0)
            return BadRequest(new { message = "Insufficient balance for payout" });

        if (request.Amount <= 0)
            return BadRequest(new { message = "Payout amount must be greater than zero" });

        if (request.Amount > vendor.Balance)
            return BadRequest(new { message = $"Insufficient balance. Available: {vendor.Balance}" });

        var pendingPayouts = await _vendorDb.Payouts
            .Where(p => p.VendorId == vendorId &&
                        (p.Status == "Pending" || p.Status == "Processing") &&
                        !p.IsDeleted)
            .ToListAsync();

        if (pendingPayouts.Any())
            return BadRequest(new { message = "A payout request is already in progress" });

        var payout = new VendorPayout
        {
            PayoutNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{vendorId:D4}-{DateTime.UtcNow.Ticks % 10000:D4}",
            Amount = request.Amount,
            Status = "Pending",
            PaymentMethod = request.PaymentMethod,
            VendorId = vendorId.Value
        };

        _vendorDb.Payouts.Add(payout);
        await _vendorDb.SaveChangesAsync();

        return Ok(new { message = "Payout request submitted", payoutId = payout.Id, payoutNumber = payout.PayoutNumber });
    }
}

public class RequestPayoutRequest
{
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
}
