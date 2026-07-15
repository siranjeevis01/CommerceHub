using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Modules.Cms.Application.Commands.Banners;
using CommerceHub.Modules.Cms.Application.Queries.Banners;

namespace CommerceHub.Modules.Cms.Presentation.Controllers;

[ApiController]
[Route("api/banners")]
public class BannerController : ControllerBase
{
    private readonly IMediator _mediator;

    public BannerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, [FromQuery] bool? isActive = null)
    {
        var query = new GetAllBannersQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IsActive = isActive
        };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var query = new GetActiveBannersQuery();
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetBannerByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        if (result is null)
            return NotFound(new { Success = false, Message = "Banner not found" });
        return Ok(new { Success = true, Data = result });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBannerCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { Success = true, Data = id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBannerCommand command)
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
        await _mediator.Send(new DeleteBannerCommand { Id = id });
        return Ok(new { Success = true, Message = "Banner deleted" });
    }
}
