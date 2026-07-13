using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Product.Infrastructure.Data;
using CommerceHub.Product.Domain.Entities;

namespace CommerceHub.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/categories")]
[Authorize(Roles = "Admin")]
public class AdminCategoryController : ControllerBase
{
    private readonly ProductDbContext _productDb;

    public AdminCategoryController(ProductDbContext productDb)
    {
        _productDb = productDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var totalCount = await _productDb.Categories.CountAsync();
        var categories = await _productDb.Categories
            .OrderBy(c => c.DisplayOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.ImageUrl,
                c.ParentCategoryId,
                c.DisplayOrder,
                productCount = c.Products.Count,
                subCategoryCount = c.SubCategories.Count,
                c.IsActive,
                c.CreatedAt
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, data = categories });
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            ParentCategoryId = request.ParentCategoryId,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _productDb.Categories.Add(category);
        await _productDb.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, new { category.Id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _productDb.Categories
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.ImageUrl,
                c.ParentCategoryId,
                c.DisplayOrder,
                c.IsActive,
                c.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (category == null) return NotFound();
        return Ok(category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _productDb.Categories.FindAsync(id);
        if (category == null) return NotFound();

        if (request.Name != null) category.Name = request.Name;
        if (request.Slug != null) category.Slug = request.Slug;
        if (request.Description != null) category.Description = request.Description;
        if (request.ImageUrl != null) category.ImageUrl = request.ImageUrl;
        if (request.ParentCategoryId.HasValue) category.ParentCategoryId = request.ParentCategoryId;
        if (request.DisplayOrder.HasValue) category.DisplayOrder = request.DisplayOrder.Value;
        if (request.IsActive.HasValue) category.IsActive = request.IsActive.Value;

        category.UpdatedAt = DateTime.UtcNow;
        await _productDb.SaveChangesAsync();
        return Ok(new { message = "Category updated" });
    }

    [HttpPut("{id}/reorder")]
    public async Task<IActionResult> ReorderCategory(int id, [FromBody] ReorderCategoryRequest request)
    {
        var category = await _productDb.Categories.FindAsync(id);
        if (category == null) return NotFound();

        category.DisplayOrder = request.SortOrder;
        category.UpdatedAt = DateTime.UtcNow;
        await _productDb.SaveChangesAsync();
        return Ok(new { message = "Category reordered" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _productDb.Categories.FindAsync(id);
        if (category == null) return NotFound();

        var hasProducts = await _productDb.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts) return BadRequest(new { message = "Cannot delete category with associated products" });

        var hasSubCategories = await _productDb.Categories.AnyAsync(c => c.ParentCategoryId == id);
        if (hasSubCategories) return BadRequest(new { message = "Cannot delete category with subcategories" });

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _productDb.SaveChangesAsync();
        return Ok(new { message = "Category deleted" });
    }
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateCategoryRequest
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
}

public class ReorderCategoryRequest
{
    public int SortOrder { get; set; }
}
