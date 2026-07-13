using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Vendor.Infrastructure.Data;
using CommerceHub.Vendor.Domain.Entities;

namespace CommerceHub.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/payouts")]
[Authorize(Roles = "Admin")]
public class AdminPayoutController : ControllerBase
{
    private readonly VendorDbContext _vendorDb;

    public AdminPayoutController(VendorDbContext vendorDb)
    {
        _vendorDb = vendorDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetPayouts(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _vendorDb.Payouts
            .Include(p => p.Vendor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);

        var totalCount = await query.CountAsync();
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
                p.VendorId,
                vendorName = p.Vendor != null ? p.Vendor.StoreName : null,
                p.CreatedAt
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, data = payouts });
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdatePayoutStatus(int id, [FromBody] UpdatePayoutStatusRequest request)
    {
        var payout = await _vendorDb.Payouts.FindAsync(id);
        if (payout == null) return NotFound();

        payout.Status = request.Status;
        if (request.TransactionId != null)
            payout.TransactionId = request.TransactionId;
        payout.UpdatedAt = DateTime.UtcNow;

        await _vendorDb.SaveChangesAsync();
        return Ok(new { message = $"Payout status updated to {request.Status}" });
    }

    [HttpPost("batch/process")]
    public async Task<IActionResult> BatchProcessPayouts([FromBody] BatchProcessPayoutRequest request)
    {
        var payouts = await _vendorDb.Payouts
            .Where(p => request.PayoutIds.Contains(p.Id) && p.Status == "Pending")
            .ToListAsync();

        foreach (var payout in payouts)
        {
            payout.Status = "Processing";
            payout.UpdatedAt = DateTime.UtcNow;
        }

        await _vendorDb.SaveChangesAsync();
        return Ok(new { message = $"{payouts.Count} payouts queued for processing" });
    }
}

public class UpdatePayoutStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
}

public class BatchProcessPayoutRequest
{
    public List<int> PayoutIds { get; set; } = new();
}
