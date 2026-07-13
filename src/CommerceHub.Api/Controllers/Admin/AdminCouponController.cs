using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Cms.Infrastructure.Data;
using CommerceHub.Cms.Domain.Entities;

namespace CommerceHub.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/coupons")]
[Authorize(Roles = "Admin")]
public class AdminCouponController : ControllerBase
{
    private readonly CmsDbContext _cmsDb;

    public AdminCouponController(CmsDbContext cmsDb)
    {
        _cmsDb = cmsDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetCoupons([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var totalCount = await _cmsDb.Coupons.CountAsync();
        var coupons = await _cmsDb.Coupons
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, data = coupons });
    }

    [HttpPost]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponRequest request)
    {
        var coupon = new Coupon
        {
            Code = request.Code,
            Type = request.Type,
            DiscountAmount = request.DiscountAmount,
            DiscountPercentage = request.DiscountPercentage,
            MinimumOrderAmount = request.MinimumOrderAmount,
            MaxUsageCount = request.MaxUsageCount,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _cmsDb.Coupons.Add(coupon);
        await _cmsDb.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCoupon), new { id = coupon.Id }, new { coupon.Id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCoupon(int id)
    {
        var coupon = await _cmsDb.Coupons.FindAsync(id);
        if (coupon == null) return NotFound();
        return Ok(coupon);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCoupon(int id, [FromBody] UpdateCouponRequest request)
    {
        var coupon = await _cmsDb.Coupons.FindAsync(id);
        if (coupon == null) return NotFound();

        if (request.Code != null) coupon.Code = request.Code;
        if (request.Type != null) coupon.Type = request.Type;
        if (request.DiscountAmount.HasValue) coupon.DiscountAmount = request.DiscountAmount;
        if (request.DiscountPercentage.HasValue) coupon.DiscountPercentage = request.DiscountPercentage;
        if (request.MinimumOrderAmount.HasValue) coupon.MinimumOrderAmount = request.MinimumOrderAmount;
        if (request.MaxUsageCount.HasValue) coupon.MaxUsageCount = request.MaxUsageCount;
        if (request.ValidFrom.HasValue) coupon.ValidFrom = request.ValidFrom;
        if (request.ValidTo.HasValue) coupon.ValidTo = request.ValidTo;
        if (request.IsActive.HasValue) coupon.IsActive = request.IsActive.Value;

        coupon.UpdatedAt = DateTime.UtcNow;
        await _cmsDb.SaveChangesAsync();
        return Ok(new { message = "Coupon updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCoupon(int id)
    {
        var coupon = await _cmsDb.Coupons.FindAsync(id);
        if (coupon == null) return NotFound();

        _cmsDb.Coupons.Remove(coupon);
        await _cmsDb.SaveChangesAsync();
        return Ok(new { message = "Coupon deleted" });
    }
}

public class CreateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int? MaxUsageCount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

public class UpdateCouponRequest
{
    public string? Code { get; set; }
    public string? Type { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int? MaxUsageCount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool? IsActive { get; set; }
}
