using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Product.Infrastructure.Data;
using CommerceHub.Vendor.Infrastructure.Data;
using ProductEntity = CommerceHub.Product.Domain.Entities.Product;

namespace CommerceHub.Api.Controllers.Vendor;

[ApiController]
[Route("api/v1/vendor/products")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly VendorDbContext _vendorDb;
    private readonly ProductDbContext _productDb;

    public ProductController(VendorDbContext vendorDb, ProductDbContext productDb)
    {
        _vendorDb = vendorDb;
        _productDb = productDb;
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
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var query = _productDb.Products
            .Where(p => p.VendorId == vendorId && !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.SKU.Contains(search));

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var totalItems = await query.CountAsync();
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
                p.ShortDescription,
                p.StockQuantity,
                p.StockStatus,
                p.MainImageUrl,
                p.IsFeatured,
                p.IsPublished,
                p.IsActive,
                p.CategoryId,
                p.BrandId,
                p.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            items = products,
            totalItems,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var product = await _productDb.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Variants)
            .Include(p => p.AttributeValues)
            .FirstOrDefaultAsync(p => p.Id == id && p.VendorId == vendorId && !p.IsDeleted);

        if (product is null) return NotFound(new { message = "Product not found" });

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        if (await _productDb.Products.AnyAsync(p => p.SKU == request.SKU && !p.IsDeleted))
            return BadRequest(new { message = "SKU already exists" });

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
            MainImageUrl = request.MainImageUrl,
            GalleryImages = request.GalleryImages,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            VendorId = vendorId.Value,
            IsPublished = request.IsPublished,
            IsActive = true
        };

        _productDb.Products.Add(product);
        await _productDb.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var product = await _productDb.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.VendorId == vendorId && !p.IsDeleted);

        if (product is null) return NotFound(new { message = "Product not found" });

        if (request.Name is not null) product.Name = request.Name;
        if (request.Slug is not null) product.Slug = request.Slug;
        if (request.Price.HasValue) product.Price = request.Price.Value;
        if (request.ComparePrice.HasValue) product.ComparePrice = request.ComparePrice;
        if (request.ShortDescription is not null) product.ShortDescription = request.ShortDescription;
        if (request.LongDescription is not null) product.LongDescription = request.LongDescription;
        if (request.StockQuantity.HasValue) product.StockQuantity = request.StockQuantity.Value;
        if (request.MainImageUrl is not null) product.MainImageUrl = request.MainImageUrl;
        if (request.GalleryImages is not null) product.GalleryImages = request.GalleryImages;
        if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId.Value;
        if (request.BrandId.HasValue) product.BrandId = request.BrandId;
        if (request.IsPublished.HasValue) product.IsPublished = request.IsPublished.Value;
        if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;

        product.UpdatedAt = DateTime.UtcNow;
        await _productDb.SaveChangesAsync();

        return Ok(new { message = "Product updated", productId = product.Id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var product = await _productDb.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.VendorId == vendorId && !p.IsDeleted);

        if (product is null) return NotFound(new { message = "Product not found" });

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _productDb.SaveChangesAsync();

        return Ok(new { message = "Product deleted" });
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkCreateOrUpdate([FromBody] BulkProductRequest request)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId is null) return NotFound(new { message = "Vendor profile not found" });

        var results = new List<object>();

        foreach (var item in request.Products)
        {
            if (item.Id is > 0)
            {
                var existing = await _productDb.Products
                    .FirstOrDefaultAsync(p => p.Id == item.Id && p.VendorId == vendorId && !p.IsDeleted);

                if (existing is null)
                {
                    results.Add(new { itemId = item.Id, success = false, message = "Not found" });
                    continue;
                }

                if (item.Name is not null) existing.Name = item.Name;
                if (item.Price.HasValue) existing.Price = item.Price.Value;
                if (item.StockQuantity.HasValue) existing.StockQuantity = item.StockQuantity.Value;
                if (item.IsPublished.HasValue) existing.IsPublished = item.IsPublished.Value;
                if (item.IsActive.HasValue) existing.IsActive = item.IsActive.Value;
                existing.UpdatedAt = DateTime.UtcNow;

                results.Add(new { itemId = item.Id, success = true, message = "Updated" });
            }
            else
            {
                if (await _productDb.Products.AnyAsync(p => p.SKU == item.SKU && !p.IsDeleted))
                {
                    results.Add(new { sku = item.SKU, success = false, message = "SKU exists" });
                    continue;
                }

                var product = new ProductEntity
                {
                    Name = item.Name ?? "",
                    Slug = item.Slug ?? "",
                    SKU = item.SKU ?? "",
                    Price = item.Price ?? 0,
                    ComparePrice = item.ComparePrice,
                    ShortDescription = item.ShortDescription,
                    LongDescription = item.LongDescription,
                    StockQuantity = item.StockQuantity ?? 0,
                    MainImageUrl = item.MainImageUrl,
                    GalleryImages = item.GalleryImages,
                    CategoryId = item.CategoryId ?? 0,
                    BrandId = item.BrandId,
                    VendorId = vendorId.Value,
                    IsPublished = item.IsPublished ?? false,
                    IsActive = item.IsActive ?? true
                };

                _productDb.Products.Add(product);
                results.Add(new { sku = item.SKU, success = true, message = "Created" });
            }
        }

        await _productDb.SaveChangesAsync();
        return Ok(new { results });
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
    public string? MainImageUrl { get; set; }
    public string? GalleryImages { get; set; }
    public int CategoryId { get; set; }
    public int? BrandId { get; set; }
    public bool IsPublished { get; set; }
}

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public decimal? Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public int? StockQuantity { get; set; }
    public string? MainImageUrl { get; set; }
    public string? GalleryImages { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public bool? IsPublished { get; set; }
    public bool? IsActive { get; set; }
}

public class BulkProductRequest
{
    public List<BulkProductItem> Products { get; set; } = new();
}

public class BulkProductItem
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? SKU { get; set; }
    public decimal? Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public int? StockQuantity { get; set; }
    public string? MainImageUrl { get; set; }
    public string? GalleryImages { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public bool? IsPublished { get; set; }
    public bool? IsActive { get; set; }
}
