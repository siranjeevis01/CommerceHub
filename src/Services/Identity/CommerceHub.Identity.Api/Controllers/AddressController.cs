using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CommerceHub.Identity.Application.Commands.Address;
using CommerceHub.Identity.Application.Queries;

namespace CommerceHub.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IMediator _mediator;

    public AddressController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("users/{userId}/addresses")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var result = await _mediator.Send(new GetAddressesByUserQuery { UserId = userId });
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("users/{userId}/addresses")]
    public async Task<IActionResult> Create(int userId, [FromBody] AddAddressCommand command)
    {
        var result = await _mediator.Send(command with { UserId = userId });
        return Ok(new { Success = true, Data = result });
    }

    [HttpPut("addresses/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAddressCommand command)
    {
        var result = await _mediator.Send(command with { Id = id });
        return Ok(new { Success = true, Data = result });
    }

    [HttpDelete("addresses/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteAddressCommand { Id = id });
        return Ok(new { Success = true, Message = "Address deleted" });
    }
}
