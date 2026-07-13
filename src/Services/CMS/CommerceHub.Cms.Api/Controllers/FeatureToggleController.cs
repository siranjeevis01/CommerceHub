using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Cms.Domain.Entities;
using CommerceHub.Cms.Infrastructure.Data;

namespace CommerceHub.Cms.Api.Controllers;

[ApiController]
[Route("api/admin/feature-toggles")]
public class FeatureToggleController : ControllerBase
{
    private readonly CmsDbContext _db;
    public FeatureToggleController(CmsDbContext db) => _db = db;

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(new { Success = true, Data = await _db.FeatureToggles.ToListAsync() });

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FeatureToggle toggle)
    {
        _db.FeatureToggles.Add(toggle);
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = toggle });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Toggle(int id)
    {
        var toggle = await _db.FeatureToggles.FindAsync(id);
        if (toggle == null) return NotFound();
        toggle.Enabled = !toggle.Enabled;
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = toggle });
    }
}
