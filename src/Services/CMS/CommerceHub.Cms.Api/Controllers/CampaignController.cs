using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Cms.Domain.Entities;
using CommerceHub.Cms.Infrastructure.Data;

namespace CommerceHub.Cms.Api.Controllers;

[ApiController]
[Route("api/admin/campaigns")]
public class CampaignController : ControllerBase
{
    private readonly CmsDbContext _db;
    public CampaignController(CmsDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(new { Success = true, Data = await _db.Campaigns.ToListAsync() });

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Campaign campaign)
    {
        _db.Campaigns.Add(campaign);
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = campaign });
    }

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
