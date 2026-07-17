using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Modules.Analytics.Application.Commands;
using CommerceHub.Modules.Analytics.Application.Interfaces;

namespace CommerceHub.Modules.Analytics.Presentation.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/reports")]
public class ReportController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IReportService _reportService;

    public ReportController(IMediator mediator, IReportService reportService)
    {
        _mediator = mediator;
        _reportService = reportService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateReportCommand command)
    {
        var bytes = await _mediator.Send(command);
        var contentType = command.Format == ReportFormat.Excel
            ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            : "application/pdf";
        var extension = command.Format == ReportFormat.Excel ? "xlsx" : "pdf";
        return File(bytes, contentType, $"Report.{extension}");
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateTime? date)
    {
        var reportDate = date ?? DateTime.UtcNow.Date;
        var fileBytes = await _reportService.GenerateDailyReportAsync(reportDate);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Daily_Report_{reportDate:yyyy-MM-dd}.xlsx");
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklyReport([FromQuery] DateTime? date)
    {
        var reportDate = date ?? DateTime.UtcNow.Date;
        var fileBytes = await _reportService.GenerateWeeklyReportAsync(reportDate);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Weekly_Report_{reportDate:yyyy-MM-dd}.xlsx");
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
    {
        if (year < 2000 || month < 1 || month > 12)
            return BadRequest("Invalid year or month");
        var fileBytes = await _reportService.GenerateMonthlyReportAsync(year, month);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Monthly_Report_{year}-{month:D2}.xlsx");
    }

    [Authorize(Roles = "Vendor,Admin,SuperAdmin")]
    [HttpGet("vendor")]
    public async Task<IActionResult> GetVendorReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var fileBytes = await _reportService.GenerateVendorReportAsync(userId, fromDate, toDate);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Vendor_Report_{fromDate:yyyy-MM-dd}_to_{toDate:yyyy-MM-dd}.xlsx");
    }
}
