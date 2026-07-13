using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Analytics.Infrastructure.Data;

namespace CommerceHub.Analytics.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/audit-logs")]
public class AuditLogController : ControllerBase
{
    private readonly AnalyticsDbContext _db;
    public AuditLogController(AnalyticsDbContext db) => _db = db;

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? entity = null, [FromQuery] int? userId = null)
    {
        var query = _db.AuditLogs.AsQueryable();
        if (!string.IsNullOrEmpty(entity)) query = query.Where(l => l.Entity == entity);
        if (userId.HasValue) query = query.Where(l => l.UserId == userId);
        
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.Timestamp).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { Success = true, Data = new { Items = items, Total = total, Page = page, PageSize = pageSize } });
    }
}
