using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Identity.Application.Commands.Otp;

namespace CommerceHub.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/otp")]
public class OtpController : ControllerBase
{
    private readonly IMediator _mediator;

    public OtpController(IMediator mediator) => _mediator = mediator;

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendOtpCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Success = true, Message = "OTP sent" });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyOtpCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Success = true, Message = "OTP verified" });
    }
}
