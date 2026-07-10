using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Vendor.Domain.Entities;
using CommerceHub.Vendor.Infrastructure.Data;

namespace CommerceHub.Vendor.Api.Controllers;

[ApiController]
[Route("api/admin/commissions")]
public class CommissionController : ControllerBase
{
    private readonly VendorDbContext _db;
    public CommissionController(VendorDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(new { Success = true, Data = await _db.Commissions.ToListAsync() });

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CommissionConfig config)
    {
        _db.Commissions.Add(config);
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = config });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CommissionConfig updated)
    {
        var config = await _db.Commissions.FindAsync(id);
        if (config == null) return NotFound();
        config.Rate = updated.Rate;
        config.Name = updated.Name;
        config.Type = updated.Type;
        await _db.SaveChangesAsync();
        return Ok(new { Success = true, Data = config });
    }
}
