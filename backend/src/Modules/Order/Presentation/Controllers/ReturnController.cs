using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CommerceHub.Modules.Order.Application.Commands;

namespace CommerceHub.Modules.Order.Presentation.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/returns")]
[Authorize]
public class ReturnController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReturnController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> RequestReturn([FromBody] ReturnOrderCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Success = true, Message = "Return request submitted" });
    }
}
