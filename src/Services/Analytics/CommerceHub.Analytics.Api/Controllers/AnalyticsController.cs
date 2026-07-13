using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Analytics.Application.Queries;
using CommerceHub.Analytics.Application.Commands;

namespace CommerceHub.Analytics.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var query = new GetDashboardSummaryQuery();
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = new GetSalesReportQuery
        {
            From = from ?? DateTime.UtcNow.AddDays(-30),
            To = to ?? DateTime.UtcNow
        };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int count = 10)
    {
        var query = new GetTopProductsQuery
        {
            From = from,
            To = to,
            Count = count
        };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserAnalytics(int userId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = new GetUserAnalyticsQuery
        {
            UserId = userId,
            From = from,
            To = to
        };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("vendors/{vendorId}/performance")]
    public async Task<IActionResult> GetVendorPerformance(int vendorId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = new GetVendorPerformanceQuery
        {
            VendorId = vendorId,
            From = from,
            To = to
        };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("track")]
    public async Task<IActionResult> TrackEvent([FromBody] TrackEventCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Success = true, Message = "Event tracked" });
    }

    [HttpPost("track/page-view")]
    public async Task<IActionResult> TrackPageView([FromBody] TrackPageViewCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Success = true, Message = "Page view tracked" });
    }
}
