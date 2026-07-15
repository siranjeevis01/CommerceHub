using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Modules.Product.Application.Commands;
using CommerceHub.Modules.Product.Application.Queries;

namespace CommerceHub.Modules.Product.Presentation.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/wishlist")]
public class WishlistController : ControllerBase
{
    private readonly IMediator _mediator;

    public WishlistController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetWishlist(int userId)
    {
        var result = await _mediator.Send(new GetWishlistQuery(userId));
        return Ok(new { Success = true, Data = result });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddWishlistItemCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id)
    {
        await _mediator.Send(new RemoveWishlistItemCommand(id));
        return Ok(new { Success = true, Message = "Removed from wishlist" });
    }
}
