using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Cms.Domain.Entities;
using CommerceHub.Modules.Cms.Infrastructure.Data;

namespace CommerceHub.Modules.Cms.Presentation.Controllers;

[ApiController]
[Route("api/admin/platform-settings")]
public class PlatformSettingController : ControllerBase
{
    private readonly CmsDbContext _db;
    public PlatformSettingController(CmsDbContext db) => _db = db;

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(new { Success = true, Data = await _db.PlatformSettings.ToListAsync() });

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PlatformSetting setting)
    {
        _db.PlatformSettings.Add(setting);
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = setting });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PlatformSetting updated)
    {
        var setting = await _db.PlatformSettings.FindAsync(id);
        if (setting == null) return NotFound();
        setting.Value = updated.Value;
        setting.Description = updated.Description;
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = setting });
    }
}
