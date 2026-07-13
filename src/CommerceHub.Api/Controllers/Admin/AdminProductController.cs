using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Product.Infrastructure.Data;
using ProductEntity = CommerceHub.Product.Domain.Entities.Product;

namespace CommerceHub.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductController : ControllerBase
{
    private readonly ProductDbContext _productDb;

    public AdminProductController(ProductDbContext productDb)
    {
        _productDb = productDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? search = null,
        [FromQuery] int? vendorId = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _productDb.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.SKU.ToLower().Contains(term));
        }

        if (vendorId.HasValue)
            query = query.Where(p => p.VendorId == vendorId.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        var totalCount = await query.CountAsync();
        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.SKU,
                p.Price,
                p.ComparePrice,
                p.StockQuantity,
                p.IsActive,
                p.IsPublished,
                p.MainImageUrl,
                p.VendorId,
                categoryName = p.Category != null ? p.Category.Name : null,
                p.CreatedAt
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, data = products });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productDb.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.SKU,
                p.Price,
                p.ComparePrice,
                p.ShortDescription,
                p.LongDescription,
                p.StockQuantity,
                p.StockStatus,
                p.MainImageUrl,
                p.GalleryImages,
                p.IsFeatured,
                p.IsPublished,
                p.IsActive,
                p.VendorId,
                p.CategoryId,
                categoryName = p.Category != null ? p.Category.Name : null,
                p.BrandId,
                brandName = p.Brand != null ? p.Brand.Name : null,
                p.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var product = new ProductEntity
        {
            Name = request.Name,
            Slug = request.Slug,
            SKU = request.SKU,
            Price = request.Price,
            ComparePrice = request.ComparePrice,
            ShortDescription = request.ShortDescription,
            LongDescription = request.LongDescription,
            StockQuantity = request.StockQuantity,
            VendorId = request.VendorId,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            MainImageUrl = request.MainImageUrl,
            IsPublished = request.IsPublished,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _productDb.Products.Add(product);
        await _productDb.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new { product.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var product = await _productDb.Products.FindAsync(id);
        if (product == null) return NotFound();

        if (request.Name != null) product.Name = request.Name;
        if (request.Slug != null) product.Slug = request.Slug;
        if (request.SKU != null) product.SKU = request.SKU;
        if (request.Price.HasValue) product.Price = request.Price.Value;
        if (request.ComparePrice.HasValue) product.ComparePrice = request.ComparePrice;
        if (request.ShortDescription != null) product.ShortDescription = request.ShortDescription;
        if (request.LongDescription != null) product.LongDescription = request.LongDescription;
        if (request.StockQuantity.HasValue) product.StockQuantity = request.StockQuantity.Value;
        if (request.MainImageUrl != null) product.MainImageUrl = request.MainImageUrl;
        if (request.IsPublished.HasValue) product.IsPublished = request.IsPublished.Value;
        if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId.Value;
        if (request.BrandId.HasValue) product.BrandId = request.BrandId;

        product.UpdatedAt = DateTime.UtcNow;
        await _productDb.SaveChangesAsync();
        return Ok(new { message = "Product updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _productDb.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _productDb.SaveChangesAsync();
        return Ok(new { message = "Product deleted" });
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateProduct(int id)
    {
        var product = await _productDb.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.IsActive = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _productDb.SaveChangesAsync();
        return Ok(new { message = "Product activated" });
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateProduct(int id)
    {
        var product = await _productDb.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _productDb.SaveChangesAsync();
        return Ok(new { message = "Product deactivated" });
    }

    [HttpPost("bulk/activate")]
    public async Task<IActionResult> BulkActivate([FromBody] BulkProductRequest request)
    {
        var products = await _productDb.Products.Where(p => request.ProductIds.Contains(p.Id)).ToListAsync();
        foreach (var p in products)
        {
            p.IsActive = true;
            p.UpdatedAt = DateTime.UtcNow;
        }
        await _productDb.SaveChangesAsync();
        return Ok(new { message = $"{products.Count} products activated" });
    }

    [HttpPost("bulk/deactivate")]
    public async Task<IActionResult> BulkDeactivate([FromBody] BulkProductRequest request)
    {
        var products = await _productDb.Products.Where(p => request.ProductIds.Contains(p.Id)).ToListAsync();
        foreach (var p in products)
        {
            p.IsActive = false;
            p.UpdatedAt = DateTime.UtcNow;
        }
        await _productDb.SaveChangesAsync();
        return Ok(new { message = $"{products.Count} products deactivated" });
    }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public int StockQuantity { get; set; }
    public int VendorId { get; set; }
    public int CategoryId { get; set; }
    public int? BrandId { get; set; }
    public string? MainImageUrl { get; set; }
    public bool IsPublished { get; set; }
}

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? SKU { get; set; }
    public decimal? Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public int? StockQuantity { get; set; }
    public string? MainImageUrl { get; set; }
    public bool? IsPublished { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
}

public class BulkProductRequest
{
    public List<int> ProductIds { get; set; } = new();
}
