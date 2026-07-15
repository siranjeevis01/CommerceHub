using MediatR;
using Microsoft.AspNetCore.Mvc;
using CommerceHub.Modules.Cart.Application.Commands;
using CommerceHub.Modules.Cart.Application.Queries;

namespace CommerceHub.Modules.Cart.Presentation.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/cart")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{cartKey}")]
    public async Task<IActionResult> GetCart(string cartKey)
    {
        var query = new GetCartQuery { CartKey = cartKey };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{cartKey}/count")]
    public async Task<IActionResult> GetItemCount(string cartKey)
    {
        var query = new GetCartItemCountQuery { CartKey = cartKey };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("{cartKey}/items")]
    public async Task<IActionResult> AddToCart(string cartKey, [FromBody] AddToCartCommand command)
    {
        command = command with { CartKey = cartKey };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPut("{cartKey}/items")]
    public async Task<IActionResult> UpdateCartItem(string cartKey, [FromBody] UpdateCartItemCommand command)
    {
        command = command with { CartKey = cartKey };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpDelete("{cartKey}/items/{productId}")]
    public async Task<IActionResult> RemoveFromCart(string cartKey, int productId)
    {
        var command = new RemoveFromCartCommand { CartKey = cartKey, ProductId = productId };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    [HttpDelete("{cartKey}")]
    public async Task<IActionResult> ClearCart(string cartKey)
    {
        var command = new ClearCartCommand { CartKey = cartKey };
        await _mediator.Send(command);
        return Ok(new { Success = true });
    }

    [HttpPost("{cartKey}/coupon")]
    public async Task<IActionResult> ApplyCoupon(string cartKey, [FromBody] ApplyCouponCommand command)
    {
        command = command with { CartKey = cartKey };
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }
}
