using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Cms.Domain.Entities;
using CommerceHub.Cms.Infrastructure.Data;

namespace CommerceHub.Cms.Api.Controllers;

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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Menu menu)
    {
        _db.Menus.Add(menu);
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = menu });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Menu updated)
    {
        var menu = await _db.Menus.FindAsync(id);
        if (menu == null) return NotFound();
        menu.Name = updated.Name;
        menu.Icon = updated.Icon;
        menu.Route = updated.Route;
        menu.DisplayOrder = updated.DisplayOrder;
        menu.IsVisible = updated.IsVisible;
        menu.ParentMenuId = updated.ParentMenuId;
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = menu });
    }

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
