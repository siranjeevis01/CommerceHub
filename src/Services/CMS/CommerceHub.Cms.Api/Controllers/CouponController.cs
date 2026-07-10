using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Cms.Application.Commands.Coupons;
using CommerceHub.Cms.Application.Queries.Coupons;

namespace CommerceHub.Cms.Api.Controllers;

[ApiController]
[Route("api/coupons")]
public class CouponController : ControllerBase
{
    private readonly IMediator _mediator;

    public CouponController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, [FromQuery] bool? isActive = null, [FromQuery] string? couponType = null)
    {
        var query = new GetAllCouponsQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IsActive = isActive,
            CouponType = couponType
        };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetCouponByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        if (result is null)
            return NotFound(new { Success = false, Message = "Coupon not found" });
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var query = new GetCouponByCodeQuery { Code = code };
        var result = await _mediator.Send(query);
        if (result is null)
            return NotFound(new { Success = false, Message = "Coupon not found" });
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCouponCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { Success = true, Data = id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCouponCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { Success = false, Message = "Id mismatch" });
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteCouponCommand { Id = id });
        return Ok(new { Success = true, Message = "Coupon deleted" });
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateCouponCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }
}
