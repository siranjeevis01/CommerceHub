using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Product.Application.Commands;
using CommerceHub.Product.Application.Queries;

namespace CommerceHub.Product.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/categories")]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllCategoriesQuery());
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryCommand command)
    {
        await _mediator.Send(command with { Id = id });
        return Ok(new { Success = true, Message = "Category updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteCategoryCommand(id));
        return Ok(new { Success = true, Message = "Category deleted" });
    }
}
