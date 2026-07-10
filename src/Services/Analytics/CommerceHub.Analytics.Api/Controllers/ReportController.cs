using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Analytics.Application.Commands;

namespace CommerceHub.Analytics.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

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
}
