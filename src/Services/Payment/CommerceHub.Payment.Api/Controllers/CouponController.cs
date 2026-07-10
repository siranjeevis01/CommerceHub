using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CommerceHub.Payment.Application.Commands;
using CommerceHub.Payment.Application.Queries;

namespace CommerceHub.Payment.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/coupons")]
public class CouponController : ControllerBase
{
    private readonly IMediator _mediator;

    public CouponController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllCouponsQuery());
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetCouponByIdQuery { Id = id });
        if (result == null)
            return NotFound(new { Success = false, Message = "Coupon not found" });
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateCouponCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("apply")]
    [Authorize]
    public async Task<IActionResult> Apply([FromBody] ApplyCouponCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(new { Success = true, Data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCouponCommand command)
    {
        command = command with { Id = id };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteCouponCommand { Id = id });
        return Ok(new { Success = true, Message = "Coupon deactivated" });
    }
}
