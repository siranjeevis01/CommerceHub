using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Product.Infrastructure.Data;
using CommerceHub.Product.Domain.Entities;

namespace CommerceHub.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/brands")]
[Authorize(Roles = "Admin")]
public class AdminBrandController : ControllerBase
{
    private readonly ProductDbContext _productDb;

    public AdminBrandController(ProductDbContext productDb)
    {
        _productDb = productDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetBrands([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var totalCount = await _productDb.Brands.CountAsync();
        var brands = await _productDb.Brands
            .OrderBy(b => b.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.LogoUrl,
                b.Description,
                productCount = b.Products.Count,
                b.IsActive,
                b.CreatedAt
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, data = brands });
    }

    [HttpPost]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest request)
    {
        var brand = new Brand
        {
            Name = request.Name,
            LogoUrl = request.LogoUrl,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _productDb.Brands.Add(brand);
        await _productDb.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBrand), new { id = brand.Id }, new { brand.Id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBrand(int id)
    {
        var brand = await _productDb.Brands
            .Where(b => b.Id == id)
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.LogoUrl,
                b.Description,
                b.IsActive,
                b.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (brand == null) return NotFound();
        return Ok(brand);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBrand(int id, [FromBody] UpdateBrandRequest request)
    {
        var brand = await _productDb.Brands.FindAsync(id);
        if (brand == null) return NotFound();

        if (request.Name != null) brand.Name = request.Name;
        if (request.LogoUrl != null) brand.LogoUrl = request.LogoUrl;
        if (request.Description != null) brand.Description = request.Description;
        if (request.IsActive.HasValue) brand.IsActive = request.IsActive.Value;

        brand.UpdatedAt = DateTime.UtcNow;
        await _productDb.SaveChangesAsync();
        return Ok(new { message = "Brand updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        var brand = await _productDb.Brands.FindAsync(id);
        if (brand == null) return NotFound();

        var hasProducts = await _productDb.Products.AnyAsync(p => p.BrandId == id);
        if (hasProducts) return BadRequest(new { message = "Cannot delete brand with associated products" });

        _productDb.Brands.Remove(brand);
        await _productDb.SaveChangesAsync();
        return Ok(new { message = "Brand deleted" });
    }
}

public class CreateBrandRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
}

public class UpdateBrandRequest
{
    public string? Name { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
