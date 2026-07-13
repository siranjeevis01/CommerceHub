using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Analytics.Infrastructure.Data;

namespace CommerceHub.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/analytics")]
[Authorize(Roles = "Admin")]
public class AdminAnalyticsController : ControllerBase
{
    private readonly AnalyticsDbContext _analyticsDb;

    public AdminAnalyticsController(AnalyticsDbContext analyticsDb)
    {
        _analyticsDb = analyticsDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetAnalytics([FromQuery] string? range = "30d")
    {
        var days = range?.ToLower() switch
        {
            "7d" => 7,
            "90d" => 90,
            "1y" => 365,
            _ => 30
        };

        var since = DateTime.UtcNow.AddDays(-days);

        var events = await _analyticsDb.AnalyticsEvents
            .Where(e => e.Timestamp >= since)
            .ToListAsync();

        var dailyData = events
            .GroupBy(e => e.Timestamp.Date)
            .Select(g => new
            {
                date = g.Key.ToString("yyyy-MM-dd"),
                totalEvents = g.Count(),
                pageViews = g.Count(e => e.EventType == "PageView"),
                uniqueUsers = g.Select(e => e.UserId).Where(u => u.HasValue).Distinct().Count()
            })
            .OrderBy(d => d.date)
            .ToList();

        var summary = new
        {
            totalEvents = events.Count,
            uniqueUsers = events.Select(e => e.UserId).Where(u => u.HasValue).Distinct().Count(),
            pageViews = events.Count(e => e.EventType == "PageView"),
            topEventTypes = events
                .GroupBy(e => e.EventType)
                .Select(g => new { eventType = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(10)
                .ToList()
        };

        return Ok(new { range, days, summary, dailyData });
    }
}
