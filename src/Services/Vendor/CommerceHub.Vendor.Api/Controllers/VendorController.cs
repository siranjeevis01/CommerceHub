using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Vendor.Application.Commands;
using CommerceHub.Vendor.Application.Queries;

namespace CommerceHub.Vendor.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/vendors")]
public class VendorController : ControllerBase
{
    private readonly IMediator _mediator;

    public VendorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null)
    {
        var query = new GetAllVendorsQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            Status = status
        };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetVendorByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var query = new GetVendorByUserIdQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendorCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVendorCommand command)
    {
        command = command with { Id = id };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var command = new ApproveVendorCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectVendorCommand command)
    {
        command = command with { Id = id };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id}/analytics")]
    public async Task<IActionResult> GetAnalytics(int id)
    {
        var query = new GetVendorAnalyticsQuery { VendorId = id };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }
}
