using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Cms.Domain.Entities;
using CommerceHub.Modules.Cms.Infrastructure.Data;

namespace CommerceHub.Modules.Cms.Presentation.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/menus")]
public class MenuController : ControllerBase
{
    private readonly CmsDbContext _db;
    public MenuController(CmsDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var menus = await _db.Menus.Include(m => m.SubMenus).OrderBy(m => m.DisplayOrder).ToListAsync();
        return Ok(new { Success = true, Data = menus });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuRequest request)
    {
        var menu = new Menu
        {
            Name = request.Name,
            DisplayOrder = request.DisplayOrder,
            Icon = request.Icon,
            Route = request.Url,
            ParentMenuId = request.ParentMenuId
        };
        _db.Menus.Add(menu);
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = menu });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMenuRequest request)
    {
        var menu = await _db.Menus.FindAsync(id);
        if (menu == null) return NotFound();
        menu.Name = request.Name;
        menu.Icon = request.Icon;
        menu.Route = request.Url;
        menu.DisplayOrder = request.DisplayOrder;
        menu.ParentMenuId = request.ParentMenuId;
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = menu });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var menu = await _db.Menus.FindAsync(id);
        if (menu == null) return NotFound();
        _db.Menus.Remove(menu);
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Message = "Menu deleted" });
    }
}

public record CreateMenuRequest(string Name, int DisplayOrder, string? Icon, string? Url, int? ParentMenuId);

public record UpdateMenuRequest(int Id, string Name, int DisplayOrder, string? Icon, string? Url, int? ParentMenuId);
