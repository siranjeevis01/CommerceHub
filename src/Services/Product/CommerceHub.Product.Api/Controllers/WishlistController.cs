using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Product.Application.Commands;
using CommerceHub.Product.Application.Queries;

namespace CommerceHub.Product.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/wishlist")]
public class WishlistController : ControllerBase
{
    private readonly IMediator _mediator;

    public WishlistController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetWishlist(int userId)
    {
        var result = await _mediator.Send(new GetWishlistQuery(userId));
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddWishlistItemCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id)
    {
        await _mediator.Send(new RemoveWishlistItemCommand(id));
        return Ok(new { Success = true, Message = "Removed from wishlist" });
    }
}
