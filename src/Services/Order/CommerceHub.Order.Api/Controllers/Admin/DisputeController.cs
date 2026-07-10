using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CommerceHub.Order.Application.Commands;

namespace CommerceHub.Order.Api.Controllers.Admin;

[ApiController]
[Route("api/v{version:apiVersion}/admin/disputes")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DisputeController : ControllerBase
{
    private readonly IMediator _mediator;

    public DisputeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("{id}/resolve")]
    public async Task<IActionResult> Resolve(int id)
    {
        await _mediator.Send(new ResolveDisputeCommand { DisputeId = id });
        return Ok(new { Success = true, Message = "Dispute resolved" });
    }
}
