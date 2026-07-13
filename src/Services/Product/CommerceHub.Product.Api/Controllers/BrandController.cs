using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Product.Application.Commands;
using CommerceHub.Product.Application.Queries;

namespace CommerceHub.Product.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/brands")]
public class BrandController : ControllerBase
{
    private readonly IMediator _mediator;

    public BrandController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllBrandsQuery());
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetBrandByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(new { Success = true, Data = result });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBrandCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBrandCommand command)
    {
        await _mediator.Send(command with { Id = id });
        return Ok(new { Success = true, Message = "Brand updated" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteBrandCommand(id));
        return Ok(new { Success = true, Message = "Brand deleted" });
    }
}
