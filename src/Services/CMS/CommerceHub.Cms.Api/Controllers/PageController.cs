using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Cms.Application.Commands.Pages;
using CommerceHub.Cms.Application.Queries.Pages;

namespace CommerceHub.Cms.Api.Controllers;

[ApiController]
[Route("api/pages")]
public class PageController : ControllerBase
{
    private readonly IMediator _mediator;

    public PageController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, [FromQuery] bool? isPublished = null)
    {
        var query = new GetAllPagesQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IsPublished = isPublished
        };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetPageByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        if (result is null)
            return NotFound(new { Success = false, Message = "Page not found" });
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var query = new GetPageBySlugQuery { Slug = slug };
        var result = await _mediator.Send(query);
        if (result is null)
            return NotFound(new { Success = false, Message = "Page not found" });
        return Ok(new { Success = true, Data = result });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePageCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { Success = true, Data = id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePageCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { Success = false, Message = "Id mismatch" });
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeletePageCommand { Id = id });
        return Ok(new { Success = true, Message = "Page deleted" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/publish")]
    public async Task<IActionResult> Publish(int id)
    {
        await _mediator.Send(new PublishPageCommand { Id = id });
        return Ok(new { Success = true, Message = "Page published" });
    }
}
