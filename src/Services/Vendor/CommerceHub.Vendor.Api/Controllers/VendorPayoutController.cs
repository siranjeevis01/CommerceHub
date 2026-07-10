using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Vendor.Application.Commands;
using CommerceHub.Vendor.Application.Queries;

namespace CommerceHub.Vendor.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/vendor-payouts")]
public class VendorPayoutController : ControllerBase
{
    private readonly IMediator _mediator;

    public VendorPayoutController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("vendor/{vendorId}")]
    public async Task<IActionResult> GetByVendor(int vendorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetVendorPayoutsQuery
        {
            VendorId = vendorId,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayout([FromBody] CreatePayoutCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("{id}/process")]
    public async Task<IActionResult> ProcessPayout(int id, [FromBody] ProcessPayoutCommand command)
    {
        command = command with { Id = id };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }
}
