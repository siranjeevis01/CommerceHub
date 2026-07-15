using CommerceHub.Shared.Contracts.Events;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Vendor.Domain.Entities;
using CommerceHub.Modules.Vendor.Infrastructure.Data;

namespace CommerceHub.Modules.Vendor.Presentation.Controllers;

[ApiController]
[Route("api/admin/settlements")]
public class SettlementController : ControllerBase
{
    private readonly VendorDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;
    public SettlementController(VendorDbContext db, IPublishEndpoint publishEndpoint)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(new { Success = true, Data = await _db.Settlements.ToListAsync() });

    [Authorize(Roles = "Admin")]
    [HttpPost("run")]
    public async Task<IActionResult> RunSettlement([FromBody] RunSettlementRequest request)
    {
        var vendors = await _db.Vendors.Where(v => v.IsActive).ToListAsync();
        foreach (var vendor in vendors)
        {
            var settlement = new Settlement
            {
                VendorId = vendor.Id,
                PeriodStart = request.PeriodStart,
                PeriodEnd = request.PeriodEnd,
                TotalAmount = vendor.TotalSales,
                TotalCommission = vendor.TotalSales * vendor.CommissionRate / 100,
                TotalEarnings = vendor.TotalSales - (vendor.TotalSales * vendor.CommissionRate / 100),
                Status = "Completed"
            };
            _db.Settlements.Add(settlement);
        }
        await _db.SaveChangesAsync();

        foreach (var vendor in vendors)
        {
            var settlementEntry = await _db.Settlements
                .FirstOrDefaultAsync(s => s.VendorId == vendor.Id && s.PeriodStart == request.PeriodStart && s.PeriodEnd == request.PeriodEnd);

            if (settlementEntry is not null)
            {
                await _publishEndpoint.Publish(new VendorSettled
                {
                    VendorId = settlementEntry.VendorId,
                    Amount = settlementEntry.TotalEarnings,
                    SettledAt = DateTime.UtcNow
                });
            }
        }

        return Ok(new { Success = true, Message = $"Settlements processed for {vendors.Count} vendors" });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("vendor/{vendorId}")]
    public async Task<IActionResult> GetVendorSummary(int vendorId)
    {
        var settlements = await _db.Settlements.Where(s => s.VendorId == vendorId).ToListAsync();
        return Ok(new { Success = true, Data = settlements });
    }
}

public record RunSettlementRequest(DateTime PeriodStart, DateTime PeriodEnd);
