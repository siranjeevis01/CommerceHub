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
    public async Task<IActionResult> Create([FromBody] CreatePlatformSettingRequest request)
    {
        var setting = new PlatformSetting
        {
            Key = request.Key,
            Value = request.Value,
            Description = request.Description
        };
        _db.PlatformSettings.Add(setting);
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = setting });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlatformSettingRequest request)
    {
        var setting = await _db.PlatformSettings.FindAsync(id);
        if (setting == null) return NotFound();
        setting.Value = request.Value;
        setting.Description = request.Description;
        setting.IsActive = request.IsEnabled;
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = setting });
    }
}

public record CreatePlatformSettingRequest(string Key, string Value, string? Description);

public record UpdatePlatformSettingRequest(int Id, string Value, string? Description, bool IsEnabled);
