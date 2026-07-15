using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Modules.Inventory.Application.Queries;
using CommerceHub.Modules.Inventory.Application.Commands;

namespace CommerceHub.Modules.Inventory.Presentation.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/warehouses")]
public class WarehouseController : ControllerBase
{
    private readonly IMediator _mediator;

    public WarehouseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllWarehousesQuery();
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetWarehouseByIdQuery(id);
        var result = await _mediator.Send(query);
        if (result is null)
            return NotFound(new { Success = false, Message = "Warehouse not found" });
        return Ok(new { Success = true, Data = result });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWarehouseCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { Success = false, Message = "Id mismatch" });
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }
}
