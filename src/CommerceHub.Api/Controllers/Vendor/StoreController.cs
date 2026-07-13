using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Vendor.Infrastructure.Data;

namespace CommerceHub.Api.Controllers.Vendor;

[ApiController]
[Route("api/v1/vendor/store")]
[Authorize]
public class StoreController : ControllerBase
{
    private readonly VendorDbContext _vendorDb;

    public StoreController(VendorDbContext vendorDb)
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
    public async Task<IActionResult> GetStoreProfile()
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var vendor = await _vendorDb.Vendors
            .Select(v => new
            {
                v.Id,
                v.StoreName,
                v.StoreDescription,
                v.StoreLogo,
                v.StoreBanner,
                v.BusinessPhone,
                v.BusinessEmail,
                v.BusinessType,
                v.BusinessAddress,
                v.VerificationStatus,
                v.CommissionRate,
                v.TotalSales,
                v.TotalEarnings,
                v.Balance,
                v.CreatedAt
            })
            .FirstOrDefaultAsync(v => v.Id == vendorId);

        if (vendor is null) return NotFound(new { message = "Vendor profile not found" });

        return Ok(vendor);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateStoreProfile([FromBody] UpdateStoreProfileRequest request)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var vendor = await _vendorDb.Vendors
            .FirstOrDefaultAsync(v => v.Id == vendorId);

        if (vendor is null) return NotFound(new { message = "Vendor profile not found" });

        if (request.StoreName is not null) vendor.StoreName = request.StoreName;
        if (request.StoreDescription is not null) vendor.StoreDescription = request.StoreDescription;
        if (request.StoreLogo is not null) vendor.StoreLogo = request.StoreLogo;
        if (request.StoreBanner is not null) vendor.StoreBanner = request.StoreBanner;
        if (request.BusinessPhone is not null) vendor.BusinessPhone = request.BusinessPhone;
        if (request.BusinessEmail is not null) vendor.BusinessEmail = request.BusinessEmail;
        if (request.BusinessType is not null) vendor.BusinessType = request.BusinessType;
        if (request.BusinessAddress is not null) vendor.BusinessAddress = request.BusinessAddress;

        vendor.UpdatedAt = DateTime.UtcNow;
        await _vendorDb.SaveChangesAsync();

        return Ok(new { message = "Store profile updated" });
    }
}

public class UpdateStoreProfileRequest
{
    public string? StoreName { get; set; }
    public string? StoreDescription { get; set; }
    public string? StoreLogo { get; set; }
    public string? StoreBanner { get; set; }
    public string? BusinessPhone { get; set; }
    public string? BusinessEmail { get; set; }
    public string? BusinessType { get; set; }
    public string? BusinessAddress { get; set; }
}
