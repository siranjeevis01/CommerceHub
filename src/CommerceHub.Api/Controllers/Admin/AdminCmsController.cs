using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Cms.Infrastructure.Data;
using CommerceHub.Cms.Domain.Entities;

namespace CommerceHub.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/cms")]
[Authorize(Roles = "Admin")]
public class AdminCmsController : ControllerBase
{
    private readonly CmsDbContext _cmsDb;

    public AdminCmsController(CmsDbContext cmsDb)
    {
        _cmsDb = cmsDb;
    }

    [HttpGet("pages")]
    public async Task<IActionResult> GetPages([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var totalCount = await _cmsDb.CmsPages.CountAsync();
        var pages = await _cmsDb.CmsPages
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Slug,
                p.IsPublished,
                p.PublishedAt,
                p.MetaTitle,
                p.CreatedAt
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, data = pages });
    }

    [HttpGet("pages/{id}")]
    public async Task<IActionResult> GetPage(int id)
    {
        var page = await _cmsDb.CmsPages.FindAsync(id);
        if (page == null) return NotFound();
        return Ok(page);
    }

    [HttpPost("pages")]
    public async Task<IActionResult> CreatePage([FromBody] CreateCmsPageRequest request)
    {
        var page = new CmsPage
        {
            Title = request.Title,
            Slug = request.Slug,
            Content = request.Content,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            IsPublished = request.IsPublished,
            PublishedAt = request.IsPublished ? DateTime.UtcNow : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _cmsDb.CmsPages.Add(page);
        await _cmsDb.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPage), new { id = page.Id }, new { page.Id });
    }

    [HttpPut("pages/{id}")]
    public async Task<IActionResult> UpdatePage(int id, [FromBody] UpdateCmsPageRequest request)
    {
        var page = await _cmsDb.CmsPages.FindAsync(id);
        if (page == null) return NotFound();

        if (request.Title != null) page.Title = request.Title;
        if (request.Slug != null) page.Slug = request.Slug;
        if (request.Content != null) page.Content = request.Content;
        if (request.MetaTitle != null) page.MetaTitle = request.MetaTitle;
        if (request.MetaDescription != null) page.MetaDescription = request.MetaDescription;
        if (request.IsPublished.HasValue)
        {
            page.IsPublished = request.IsPublished.Value;
            if (request.IsPublished.Value && page.PublishedAt == null)
                page.PublishedAt = DateTime.UtcNow;
        }

        page.UpdatedAt = DateTime.UtcNow;
        await _cmsDb.SaveChangesAsync();
        return Ok(new { message = "Page updated" });
    }

    [HttpDelete("pages/{id}")]
    public async Task<IActionResult> DeletePage(int id)
    {
        var page = await _cmsDb.CmsPages.FindAsync(id);
        if (page == null) return NotFound();

        _cmsDb.CmsPages.Remove(page);
        await _cmsDb.SaveChangesAsync();
        return Ok(new { message = "Page deleted" });
    }
}

public class CreateCmsPageRequest
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public bool IsPublished { get; set; }
}

public class UpdateCmsPageRequest
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Content { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public bool? IsPublished { get; set; }
}
