using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CommerceHub.Order.Application.Commands;
using CommerceHub.Order.Application.Queries;

namespace CommerceHub.Order.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int UserId => int.Parse(User.FindFirst("userId")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllOrdersQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery { OrderId = id });
        if (result == null) return NotFound();
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("number/{orderNumber}")]
    public async Task<IActionResult> GetByNumber(string orderNumber)
    {
        var result = await _mediator.Send(new GetOrderByNumberQuery { OrderNumber = orderNumber });
        if (result == null) return NotFound();
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetOrdersByUserQuery { UserId = userId, Page = page, PageSize = pageSize });
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("vendor/{vendorId}")]
    public async Task<IActionResult> GetByVendor(int vendorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetOrdersByVendorQuery { VendorId = vendorId, Page = page, PageSize = pageSize });
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetStatus(int id)
    {
        var result = await _mediator.Send(new GetOrderStatusQuery { OrderId = id });
        if (result == null) return NotFound();
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var result = await _mediator.Send(new GetOrderHistoryQuery { OrderId = id });
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderCommand command)
    {
        var result = await _mediator.Send(command with { UserId = UserId });
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelOrderCommand command)
    {
        await _mediator.Send(command with { OrderId = id });
        return Ok(new { Success = true, Message = "Order cancelled" });
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        await _mediator.Send(new ConfirmOrderCommand { OrderId = id });
        return Ok(new { Success = true, Message = "Order confirmed" });
    }

    [HttpPost("{id}/ship")]
    public async Task<IActionResult> Ship(int id, [FromBody] ShipOrderCommand command)
    {
        await _mediator.Send(command with { OrderId = id });
        return Ok(new { Success = true, Message = "Order shipped" });
    }

    [HttpPost("{id}/deliver")]
    public async Task<IActionResult> Deliver(int id, [FromBody] DeliverOrderCommand command)
    {
        await _mediator.Send(command with { OrderId = id });
        return Ok(new { Success = true, Message = "Order delivered" });
    }
}
