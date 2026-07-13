using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Product.Infrastructure.Data;

namespace CommerceHub.Api.Controllers.Vendor;

[ApiController]
[Route("api/v1/vendor/reviews")]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly ProductDbContext _productDb;
    private readonly CommerceHub.Vendor.Infrastructure.Data.VendorDbContext _vendorDb;

    public ReviewController(ProductDbContext productDb, CommerceHub.Vendor.Infrastructure.Data.VendorDbContext vendorDb)
    {
        _productDb = productDb;
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
        [FromQuery] int? rating = null)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var productIds = await _productDb.Products
            .Where(p => p.VendorId == vendorId && !p.IsDeleted)
            .Select(p => p.Id)
            .ToListAsync();

        var query = _productDb.Reviews
            .Where(r => productIds.Contains(r.ProductId));

        if (rating.HasValue)
            query = query.Where(r => r.Rating == rating.Value);

        var totalItems = await query.CountAsync();
        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(r => r.Product)
            .Select(r => new
            {
                reviewId = r.Id,
                productId = r.ProductId,
                productName = r.Product.Name,
                rating = r.Rating,
                comment = r.Comment,
                images = r.Images,
                isVerifiedPurchase = r.IsVerifiedPurchase,
                userId = r.UserId,
                createdAt = r.CreatedAt
            })
            .ToListAsync();

        var averageRating = productIds.Count > 0
            ? await _productDb.Reviews
                .Where(r => productIds.Contains(r.ProductId))
                .AverageAsync(r => (double?)r.Rating) ?? 0
            : 0;

        return Ok(new
        {
            items = reviews,
            totalItems,
            averageRating = Math.Round(averageRating, 2),
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        });
    }

    [HttpPut("{id}/reply")]
    public async Task<IActionResult> ReplyToReview(int id, [FromBody] ReviewReplyRequest request)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var review = await _productDb.Reviews
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null) return NotFound(new { message = "Review not found" });

        if (review.Product.VendorId != vendorId)
            return Forbid();

        review.Comment = $"{review.Comment}\n\n[Vendor Reply]: {request.Message}";
        review.UpdatedAt = DateTime.UtcNow;

        await _productDb.SaveChangesAsync();

        return Ok(new { message = "Reply added to review", reviewId = review.Id });
    }
}

public class ReviewReplyRequest
{
    public string Message { get; set; } = string.Empty;
}
