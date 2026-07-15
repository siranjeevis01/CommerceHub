using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Modules.Cms.Domain.Entities;
using CommerceHub.Modules.Cms.Infrastructure.Data;

namespace CommerceHub.Modules.Cms.Presentation.Controllers;

[ApiController]
[Route("api/admin/campaigns")]
public class CampaignController : ControllerBase
{
    private readonly CmsDbContext _db;
    public CampaignController(CmsDbContext db) => _db = db;

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(new { Success = true, Data = await _db.Campaigns.ToListAsync() });

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Campaign campaign)
    {
        _db.Campaigns.Add(campaign);
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = campaign });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Campaign updated)
    {
        var campaign = await _db.Campaigns.FindAsync(id);
        if (campaign == null) return NotFound();
        campaign.Name = updated.Name;
        campaign.DiscountPercentage = updated.DiscountPercentage;
        campaign.FixedDiscount = updated.FixedDiscount;
        campaign.StartDate = updated.StartDate;
        campaign.EndDate = updated.EndDate;
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = campaign });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var campaign = await _db.Campaigns.FindAsync(id);
        if (campaign == null) return NotFound();
        _db.Campaigns.Remove(campaign);
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Message = "Campaign deleted" });
    }
}
